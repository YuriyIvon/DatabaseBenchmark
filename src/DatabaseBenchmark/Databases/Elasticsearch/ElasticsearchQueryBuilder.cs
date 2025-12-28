using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Elasticsearch.Interfaces;
using DatabaseBenchmark.Model;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Query = Elastic.Clients.Elasticsearch.QueryDsl.Query;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public class ElasticsearchQueryBuilder : IElasticsearchQueryBuilder
    {
        private static readonly QueryPrimitiveOperator[] AllowedArrayOperators =
        [
            QueryPrimitiveOperator.Equals,
            QueryPrimitiveOperator.NotEquals,
            QueryPrimitiveOperator.Contains
        ];

        private readonly Table _table;
        private readonly DatabaseBenchmark.Model.Query _query;
        private readonly IRandomValueProvider _randomValueProvider;
        private readonly IRandomPrimitives _randomPrimitives;

        public ElasticsearchQueryBuilder(
            Table table,
            DatabaseBenchmark.Model.Query query,
            IRandomValueProvider randomValueProvider,
            IRandomPrimitives randomPrimitives)
        {
            _table = table;
            _query = query;
            _randomValueProvider = randomValueProvider;
            _randomPrimitives = randomPrimitives;
        }

        public SearchRequest Build()
        {
            _randomValueProvider?.Next();

            if (_query.Distinct)
            {
                throw new InputArgumentException("Distinct queries are not yet supported for Elasticsearch");
            }

            var request = new SearchRequest(_table.Name);

            if (_query.Columns != null)
            {
                request.Fields = _query.Columns.Select(c => new FieldAndFormat { Field = new Field(c) }).ToArray();
            }

            if (_query.Condition != null)
            {
                request.Query = BuildCondition(_query.Condition);
            }

            if (_query.Aggregate != null)
            {
                request.Aggregations = BuildAggregations();
                //TODO: make it work properly with Skip and Take
                request.Size = 0;
            }

            if (_query.Sort != null && _query.Aggregate == null)
            {
                request.Sort = _query.Sort.Select(s =>
                    new SortOptions
                    {
                        Field = new FieldSort(new Field(s.ColumnName))
                        {
                            Order = s.Direction == QuerySortDirection.Ascending
                                ? SortOrder.Asc
                                : SortOrder.Desc
                        }
                    })
                    .ToArray();
            }

            if (_query.Skip > 0)
            {
                request.From = _query.Skip;
            }

            if (_query.Take > 0)
            {
                request.Size = _query.Take;
            }

            return request;
        }

        private Query BuildCondition(IQueryCondition condition)
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

        private Query BuildGroupCondition(QueryGroupCondition condition)
        {
            var conditions = condition.Conditions
                .Select(BuildCondition)
                .Where(p => p != null)
                .ToArray();

            if (conditions.Any())
            {
                return condition.Operator switch
                {
                    QueryGroupOperator.And => new BoolQuery { Must = conditions },
                    QueryGroupOperator.Or => new BoolQuery { Should = conditions },
                    QueryGroupOperator.Not => BuildNotCondition(conditions),
                    _ => throw new InputArgumentException($"Unknown group operator \"{condition.Operator}\"")
                };
            }
            else
            {
                return null;
            }
        }

        private static Query BuildNotCondition(Query[] inputConditions)
        {
            if (inputConditions.Length > 1)
            {
                throw new InputArgumentException($"Operator \"Not\" can have only one operand");
            }

            return new BoolQuery { MustNot = inputConditions };
        }

        private Query BuildPrimitiveCondition(QueryPrimitiveCondition condition)
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

            //TODO: double-check DateTime and serialization for all database types
            return condition.Operator switch
            {
                QueryPrimitiveOperator.Equals =>
                    rawValue != null
                        ? column.Array
                            ? new TermsQuery { Field = condition.ColumnName, Terms = BuildTermsQueryField(condition.ColumnName, rawValue) }
                            : new TermQuery { Field = condition.ColumnName, Value = FieldValue.FromValue(rawValue) }
                        : new BoolQuery
                        {
                            MustNot = new List<Query>
                            {
                                new ExistsQuery { Field = condition.ColumnName }
                            }
                        },
                QueryPrimitiveOperator.In => 
                    new TermsQuery { Field = condition.ColumnName, Terms = BuildTermsQueryField(condition.ColumnName, rawValue) },
                QueryPrimitiveOperator.NotEquals =>
                    rawValue != null
                        ? new BoolQuery
                        {
                            MustNot =
                            {
                                column.Array
                                    ? new TermsQuery { Field = condition.ColumnName, Terms = BuildTermsQueryField(condition.ColumnName, rawValue) }
                                    : new TermQuery { Field = condition.ColumnName, Value = FieldValue.FromValue(rawValue) }
                            }
                        }
                        : new ExistsQuery { Field = condition.ColumnName },
                QueryPrimitiveOperator.Lower => column.Type switch
                {
                    ColumnType.Integer => new NumberRangeQuery { Field = condition.ColumnName, Lt = (long?)rawValue },
                    ColumnType.Double => new NumberRangeQuery { Field = condition.ColumnName, Lt = (double?)rawValue },
                    ColumnType.DateTime => new DateRangeQuery { Field = condition.ColumnName, Lt = DateMath.Anchored((DateTime)rawValue) },
                    ColumnType.String => new TermRangeQuery { Field = condition.ColumnName, Lt = rawValue?.ToString() },
                    ColumnType.Text => new TermRangeQuery { Field = condition.ColumnName, Lt = rawValue?.ToString() },
                    _ => throw new InputArgumentException($"Operator Lower is not supported for column type \"{column.Type}\"")
                },
                QueryPrimitiveOperator.LowerEquals => column.Type switch
                {
                    ColumnType.Integer => new NumberRangeQuery { Field = condition.ColumnName, Lte = (long?)rawValue },
                    ColumnType.Double => new NumberRangeQuery { Field = condition.ColumnName, Lte = (double?)rawValue },
                    ColumnType.DateTime => new DateRangeQuery { Field = condition.ColumnName, Lte = DateMath.Anchored((DateTime)rawValue) },
                    ColumnType.String => new TermRangeQuery { Field = condition.ColumnName, Lte = rawValue?.ToString() },
                    ColumnType.Text => new TermRangeQuery { Field = condition.ColumnName, Lte = rawValue?.ToString() },
                    _ => throw new InputArgumentException($"Operator LowerEquals is not supported for column type \"{column.Type}\"")
                },
                QueryPrimitiveOperator.Greater => column.Type switch
                {
                    ColumnType.Integer => new NumberRangeQuery { Field = condition.ColumnName, Gt = (long?)rawValue },
                    ColumnType.Double => new NumberRangeQuery { Field = condition.ColumnName, Gt = (double?)rawValue },
                    ColumnType.DateTime => new DateRangeQuery { Field = condition.ColumnName, Gt = DateMath.Anchored((DateTime)rawValue) },
                    ColumnType.String => new TermRangeQuery { Field = condition.ColumnName, Gt = rawValue?.ToString() },
                    ColumnType.Text => new TermRangeQuery { Field = condition.ColumnName, Gt = rawValue?.ToString() },
                    _ => throw new InputArgumentException($"Operator Greater is not supported for column type \"{column.Type}\"")
                },
                QueryPrimitiveOperator.GreaterEquals => column.Type switch
                {
                    ColumnType.Integer => new NumberRangeQuery { Field = condition.ColumnName, Gte = (long?)rawValue },
                    ColumnType.Double => new NumberRangeQuery { Field = condition.ColumnName, Gte = (double?)rawValue },
                    ColumnType.DateTime => new DateRangeQuery { Field = condition.ColumnName, Gte = DateMath.Anchored((DateTime)rawValue) },
                    ColumnType.String => new TermRangeQuery { Field = condition.ColumnName, Gte = rawValue?.ToString() },
                    ColumnType.Text => new TermRangeQuery { Field = condition.ColumnName, Gte = rawValue?.ToString() },
                    _ => throw new InputArgumentException($"Operator GreaterEquals is not supported for column type \"{column.Type}\"")
                },
                QueryPrimitiveOperator.Contains =>
                    column.Array
                        ? new TermQuery { Field = condition.ColumnName, Value = FieldValue.FromValue(rawValue) }
                        : new WildcardQuery { Field = condition.ColumnName, Value = $"*{rawValue}*" },
                QueryPrimitiveOperator.StartsWith => new WildcardQuery { Field = condition.ColumnName, Value = $"{rawValue}*" },
                _ => throw new InputArgumentException($"Unknown primitive operator \"{condition.Operator}\"")
            };
        }

        private IDictionary<string, Aggregation> BuildAggregations()
        {
            var sources = _query.Aggregate.GroupColumnNames
                .Select(columnName => new KeyValuePair<string, CompositeAggregationSource>(
                    columnName,
                    new CompositeTermsAggregation
                    {
                        Field = columnName,
                        Order = BuildGroupSortDirection(columnName)
                    }))
                .ToList();

            var subAggregations = _query.Aggregate.ResultColumns
                .Where(c => c.Function != QueryAggregateFunction.Count)
                .ToDictionary(c => c.ResultColumnName, BuildAggregation);

            var groupingAggregation = new Aggregation
            {
                Composite = new CompositeAggregation
                {
                    Sources = sources,
                    //TODO: parameterize and provide a way to fetch all
                    Size = 10000,
                },
                Aggregations = subAggregations
            };

            return new Dictionary<string, Aggregation>
            {
                { "grouping", groupingAggregation }
            };
        }

        private Aggregation BuildAggregation(QueryAggregateColumn resultColumn) =>
            resultColumn.Function switch
            {
                QueryAggregateFunction.Average => new AverageAggregation { Field = resultColumn.SourceColumnName },
                QueryAggregateFunction.Sum => new SumAggregation { Field = resultColumn.SourceColumnName },
                QueryAggregateFunction.Min => new MinAggregation { Field = resultColumn.SourceColumnName },
                QueryAggregateFunction.Max => new MaxAggregation { Field = resultColumn.SourceColumnName },
                _ => throw new InputArgumentException($"Unknown aggregate function {resultColumn.Function}")
            };

        private SortOrder? BuildGroupSortDirection(string groupColumnName)
        {
            var sortItem = _query.Sort?.FirstOrDefault(s => s.ColumnName == groupColumnName);
            return sortItem?.Direction == QuerySortDirection.Ascending
                ? SortOrder.Asc
                : SortOrder.Desc;
        }

        private static TermsQueryField BuildTermsQueryField(string columnName, object rawValue)
        {
            if (rawValue is IEnumerable<object> collection)
            {
                return new TermsQueryField(collection.Select(FieldValue.FromValue).ToArray());
            }
            else
            {
                throw new InputArgumentException($"The query argument for the column \"{columnName}\" must be a collection");
            }
        }

        protected Column GetColumn(string columnName)
        {
            var column = _table.Columns.FirstOrDefault(c => c.Name == columnName);
            return column ?? throw new InputArgumentException($"Unknown column \"{columnName}\"");
        }
    }
}
