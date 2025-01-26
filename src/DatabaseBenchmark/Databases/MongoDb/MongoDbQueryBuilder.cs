using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.MongoDb.Interfaces;
using DatabaseBenchmark.Model;
using MongoDB.Bson;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public class MongoDbQueryBuilder : IMongoDbQueryBuilder
    {
        private static readonly QueryPrimitiveOperator[] AllowedArrayOperators =
        [
            QueryPrimitiveOperator.Equals,
            QueryPrimitiveOperator.NotEquals,
            QueryPrimitiveOperator.Contains
        ];

        private readonly Table _table;
        private readonly Query _query;
        private readonly IRandomValueProvider _randomValueProvider;
        private readonly IRandomPrimitives _randomPrimitives;

        public MongoDbQueryBuilder(
            Table table,
            Query query,
            IRandomValueProvider randomValueProvider,
            IRandomPrimitives randomPrimitives)
        {
            _table = table;
            _query = query;
            _randomValueProvider = randomValueProvider;
            _randomPrimitives = randomPrimitives;
        }

        public IEnumerable<BsonDocument> Build()
        {
            _randomValueProvider?.Next();

            var request = new List<BsonDocument>();

            if (_query.Condition != null)
            {
                var condition = BuildCondition(_query.Condition);
                if (condition != null)
                {
                    request.Add(new BsonDocument("$match", condition));
                }
            }

            if (_query.Aggregate != null)
            {
                request.Add(new BsonDocument("$group", BuildAggregate()));
            }

            if (_query.Sort != null)
            {
                request.Add(new BsonDocument("$sort",
                    new BsonDocument(_query.Sort.ToDictionary(
                        s => BuildColumnReference(s.ColumnName),
                        s => s.Direction == QuerySortDirection.Ascending ? 1 : -1))));
            }

            if (_query.Skip > 0)
            {
                request.Add(new BsonDocument("$skip", _query.Skip));
            }

            if (_query.Take > 0)
            {
                request.Add(new BsonDocument("$limit", _query.Take));
            }

            if (_query.Columns != null || _query.Aggregate != null)
            {
                request.Add(new BsonDocument("$project", BuildProjection()));
            }

            if (_query.Distinct)
            {
                request.AddRange(BuildDistinctAggregate());
            }

            return request;
        }

        private BsonDocument BuildCondition(IQueryCondition condition)
        {
            if (condition.RandomizeInclusion && _randomPrimitives.GetRandomBoolean())
            {
                return null;
            }
            else
            {
                return condition switch
                {
                    QueryGroupCondition groupCondition => BuildGroupCondition(groupCondition),
                    QueryPrimitiveCondition primitiveCondition => BuildPrimitiveCondition(primitiveCondition),
                    _ => throw new InputArgumentException($"Unknown predicate type \"{condition.GetType()}\"")
                };
            }
        }

        private BsonDocument BuildGroupCondition(QueryGroupCondition condition)
        {
            var conditions = condition.Conditions
                .Select(BuildCondition)
                .Where(p => p != null)
                .ToArray();

            if (conditions.Any())
            {
                return condition.Operator switch
                {
                    QueryGroupOperator.And => new BsonDocument("$and", new BsonArray(conditions)),
                    QueryGroupOperator.Or => new BsonDocument("$or", new BsonArray(conditions)),
                    QueryGroupOperator.Not => new BsonDocument("$nor", new BsonArray(conditions)),
                    _ => throw new InputArgumentException($"Unknown group operator \"{condition.Operator}\"")
                };
            }
            else
            {
                return null;
            }
        }

        private BsonDocument BuildPrimitiveCondition(QueryPrimitiveCondition condition)
        {
            var column = GetColumn(condition.ColumnName);
            if (column.Array && !AllowedArrayOperators.Contains(condition.Operator))
            {
                throw new InputArgumentException($"Primitive operator \"{condition.Operator}\" is not supported for array columns");
            }

            var rawValue = !condition.RandomizeValue
                ? condition.Value
                : condition.Operator == QueryPrimitiveOperator.In
                    ? _randomValueProvider.GetValueCollection(_table.Name, column, condition.ValueRandomizationRule)
                    : _randomValueProvider.GetValue(_table.Name, column, condition.ValueRandomizationRule);

            var bsonValue = BsonValue.Create(rawValue);

            return condition.Operator switch
            {
                QueryPrimitiveOperator.Equals => new BsonDocument(condition.ColumnName, bsonValue),
                QueryPrimitiveOperator.NotEquals => new BsonDocument(condition.ColumnName, new BsonDocument("$ne", bsonValue)),
                QueryPrimitiveOperator.In => new BsonDocument(condition.ColumnName, new BsonDocument("$in", bsonValue)),
                QueryPrimitiveOperator.Lower => new BsonDocument(condition.ColumnName, new BsonDocument("$lt", bsonValue)),
                QueryPrimitiveOperator.LowerEquals => new BsonDocument(condition.ColumnName, new BsonDocument("$lte", bsonValue)),
                QueryPrimitiveOperator.Greater => new BsonDocument(condition.ColumnName, new BsonDocument("$gt", bsonValue)),
                QueryPrimitiveOperator.GreaterEquals => new BsonDocument(condition.ColumnName, new BsonDocument("$gte", bsonValue)),
                QueryPrimitiveOperator.Contains => BuildContainsCondition(condition, bsonValue),
                QueryPrimitiveOperator.StartsWith => new BsonDocument(condition.ColumnName, new BsonDocument("$regex", $"^{rawValue}")),
                _ => throw new InputArgumentException($"Unknown primitive operator \"{condition.Operator}\"")
            };
        }

        private BsonDocument BuildAggregate()
        {
            var aggregate = _query.Aggregate.GroupColumnNames.Length > 1
                ? new BsonDocument("_id", new BsonDocument(_query.Aggregate.GroupColumnNames.ToDictionary(c => c, c => $"${c}")))
                : new BsonDocument("_id", $"${_query.Aggregate.GroupColumnNames.First()}");

            foreach (var c in _query.Aggregate.ResultColumns)
            {
                var expression = c.Function switch
                {
                    QueryAggregateFunction.Average => new BsonDocument("$avg", $"${c.SourceColumnName}"),
                    QueryAggregateFunction.Count => new BsonDocument("$sum", 1),
                    QueryAggregateFunction.Max => new BsonDocument("$max", $"${c.SourceColumnName}"),
                    QueryAggregateFunction.Min => new BsonDocument("$min", $"${c.SourceColumnName}"),
                    QueryAggregateFunction.Sum => new BsonDocument("$sum", $"${c.SourceColumnName}"),
                    _ => throw new InputArgumentException($"Unknown aggregate function \"{c.Function}\"")
                };

                aggregate.Add(c.ResultColumnName, expression);
            }

            return aggregate;
        }

        private IEnumerable<BsonDocument> BuildDistinctAggregate()
        {
            if (_query.Columns == null)
            {
                throw new InputArgumentException("Columns must be explicitly specified if the distinct flag is set");
            }

            var items = new List<BsonDocument>();

            var groupColumns = new BsonDocument(_query.Columns.ToDictionary(c => c, c => $"${c}"));
            var groupKey = new BsonDocument("_id", groupColumns);
            items.Add(new BsonDocument("$group", groupKey));

            var projectionColumns = new BsonDocument()
            {
                { "_id", 0 }
            };

            projectionColumns.AddRange(_query.Columns.ToDictionary(c => c, c => $"$_id.{c}"));
            items.Add(new BsonDocument("$project", projectionColumns));

            return items;
        }

        public BsonDocument BuildProjection()
        {
            var projection = new BsonDocument()
            {
                { "_id", 0 }
            };

            var columns = new List<string>(_query.Columns);

            if (_query.Aggregate != null)
            {
                columns.AddRange(_query.Aggregate.ResultColumns.Select(c => c.ResultColumnName));
            }

            projection.AddRange(columns.ToDictionary(c => c, c => $"${BuildColumnReference(c)}"));

            return projection;
        }

        private BsonDocument BuildContainsCondition(QueryPrimitiveCondition condition, BsonValue bsonValue)
        {
            var column = GetColumn(condition.ColumnName);
            if (column.Array)
            {
                return new BsonDocument(condition.ColumnName, bsonValue);
            }
            else
            {
                return new BsonDocument(condition.ColumnName, new BsonDocument("$regex", bsonValue));
            }
        }

        private string BuildColumnReference(string columnName)
        {
            if (_query.Aggregate != null && _query.Aggregate.GroupColumnNames.Contains(columnName))
            {
                var isSingle = _query.Aggregate.GroupColumnNames.Length == 1;
                return isSingle ? "_id" : $"_id.{columnName}";
            }

            return columnName;
        }

        protected Column GetColumn(string columnName)
        {
            var column = _table.Columns.FirstOrDefault(c => c.Name == columnName);
            return column ?? throw new InputArgumentException($"Unknown column \"{columnName}\"");
        }
    }
}
