using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Elasticsearch.Interfaces;
using DatabaseBenchmark.Model;
using Nest;

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
        private readonly Query _query;
        private readonly IRandomValueProvider _randomValueProvider;
        private readonly IRandomPrimitives _randomPrimitives;

        public ElasticsearchQueryBuilder(
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
                request.Fields = _query.Columns.Select(c => new Field(c)).ToArray();
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
                    new FieldSort 
                    {
                        Field = s.ColumnName,
                        Order = s.Direction == QuerySortDirection.Ascending 
                            ? SortOrder.Ascending 
                            : SortOrder.Descending 
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

        private QueryContainer BuildCondition(IQueryCondition condition)
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

        private QueryContainer BuildGroupCondition(QueryGroupCondition condition)
        {
            var conditions = condition.Conditions
                .Select(p => BuildCondition(p))
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

        private static QueryContainer BuildNotCondition(QueryContainer[] inputConditions)
        {
            if (inputConditions.Length > 1)
            {
                throw new InputArgumentException($"Operator \"Not\" can have only one operand");
            }

            return new BoolQuery { MustNot = inputConditions };
        }

        private QueryContainer BuildPrimitiveCondition(QueryPrimitiveCondition condition)
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
                            ? new TermsQuery { Field = condition.ColumnName, Terms = (IEnumerable<object>)rawValue }
                            : new TermQuery { Field = condition.ColumnName, Value = rawValue }
                        : new BoolQuery
                        {
                            MustNot = new QueryContainer[]
                            {
                                new ExistsQuery { Field = condition.ColumnName }
                            }
                        },
                QueryPrimitiveOperator.In => new TermQuery { Field = condition.ColumnName, Value = rawValue },
                QueryPrimitiveOperator.NotEquals => 
                    rawValue != null 
                        ? new BoolQuery 
                        {
                            MustNot = new QueryContainer[]
                            {
                                column.Array
                                    ? new TermsQuery { Field = condition.ColumnName, Terms = (IEnumerable<object>)rawValue }
                                    : new TermQuery { Field = condition.ColumnName, Value = rawValue }
                            }
                        }
                        : new ExistsQuery { Field = condition.ColumnName },
                QueryPrimitiveOperator.Lower => column.Type switch 
                    {
                        ColumnType.Integer => new LongRangeQuery { Field = condition.ColumnName, LessThan = (long?)rawValue },
                        ColumnType.Double => new NumericRangeQuery { Field = condition.ColumnName, LessThan = (double?)rawValue },
                        ColumnType.DateTime => new DateRangeQuery { Field = condition.ColumnName, LessThan = (DateTime?)rawValue },
                        ColumnType.String => new TermRangeQuery { Field = condition.ColumnName, LessThan = (string)rawValue },
                        ColumnType.Text => new TermRangeQuery { Field = condition.ColumnName, LessThan = (string)rawValue },
                        _ => throw new InputArgumentException($"Operator Lower is not supported for column type \"{column.Type}\"")
                    },
                QueryPrimitiveOperator.LowerEquals => column.Type switch
                    {
                        ColumnType.Integer => new LongRangeQuery { Field = condition.ColumnName, LessThanOrEqualTo = (long?)rawValue },
                        ColumnType.Double => new NumericRangeQuery { Field = condition.ColumnName, LessThanOrEqualTo = (double?)rawValue },
                        ColumnType.DateTime => new DateRangeQuery { Field = condition.ColumnName, LessThanOrEqualTo = (DateTime?)rawValue },
                        ColumnType.String => new TermRangeQuery { Field = condition.ColumnName, LessThanOrEqualTo = (string)rawValue },
                        ColumnType.Text => new TermRangeQuery { Field = condition.ColumnName, LessThanOrEqualTo = (string)rawValue },
                        _ => throw new InputArgumentException($"Operator LowerEquals is not supported for column type \"{column.Type}\"")
                    },
                QueryPrimitiveOperator.Greater => column.Type switch
                    {
                        ColumnType.Integer => new LongRangeQuery { Field = condition.ColumnName, GreaterThan = (long?)rawValue },
                        ColumnType.Double => new NumericRangeQuery { Field = condition.ColumnName, GreaterThan = (double?)rawValue },
                        ColumnType.DateTime => new DateRangeQuery { Field = condition.ColumnName, GreaterThan = (DateTime?)rawValue },
                        ColumnType.String => new TermRangeQuery { Field = condition.ColumnName, GreaterThan = (string)rawValue },
                        ColumnType.Text => new TermRangeQuery { Field = condition.ColumnName, GreaterThan = (string)rawValue },
                        _ => throw new InputArgumentException($"Operator Greater is not supported for column type \"{column.Type}\"")
                    },
                QueryPrimitiveOperator.GreaterEquals => column.Type switch
                    {
                        ColumnType.Integer => new LongRangeQuery { Field = condition.ColumnName, GreaterThanOrEqualTo = (long?)rawValue },
                        ColumnType.Double => new NumericRangeQuery { Field = condition.ColumnName, GreaterThanOrEqualTo = (double?)rawValue },
                        ColumnType.DateTime => new DateRangeQuery { Field = condition.ColumnName, GreaterThanOrEqualTo = (DateTime?)rawValue },
                        ColumnType.String => new TermRangeQuery { Field = condition.ColumnName, GreaterThanOrEqualTo = (string)rawValue },
                        ColumnType.Text => new TermRangeQuery { Field = condition.ColumnName, GreaterThanOrEqualTo = (string)rawValue },
                        _ => throw new InputArgumentException($"Operator GreaterEquals is not supported for column type \"{column.Type}\"")
                    },
                QueryPrimitiveOperator.Contains => 
                    column.Array 
                        ? new TermQuery { Field = condition.ColumnName, Value = rawValue }
                        : new WildcardQuery { Field = condition.ColumnName, Value = $"*{rawValue}*" },
                QueryPrimitiveOperator.StartsWith => new WildcardQuery { Field = condition.ColumnName, Value = $"{rawValue}*"},
                _ => throw new InputArgumentException($"Unknown primitive operator \"{condition.Operator}\"")
            };
        }

        private AggregationDictionary BuildAggregations()
        {
            var aggregation = new CompositeAggregation("grouping")
            {
                Sources = _query.Aggregate.GroupColumnNames
                    .Select(g => new TermsCompositeAggregationSource(g)
                    { 
                        Field = g,
                        Order = BuildGroupSortDirection(g)
                    })
                    .ToArray(),
                Aggregations = new AggregationDictionary(
                    _query.Aggregate.ResultColumns
                        .Where(c => c.Function != QueryAggregateFunction.Count)
                        .ToDictionary(c => c.ResultColumnName, BuildAggregation)),
                //TODO: parameterize and provide a way to fetch all
                Size = 10000
            };

            return new AggregationDictionary(
                new Dictionary<string, AggregationContainer>
                { 
                    { "grouping", aggregation } 
                });
        }

        private static AggregationContainer BuildAggregation(QueryAggregateColumn resultColumn) =>
            resultColumn.Function switch
            {
                QueryAggregateFunction.Average => new AverageAggregation(resultColumn.ResultColumnName, resultColumn.SourceColumnName),
                QueryAggregateFunction.Sum => new SumAggregation(resultColumn.ResultColumnName, resultColumn.SourceColumnName),
                QueryAggregateFunction.Min => new MinAggregation(resultColumn.ResultColumnName, resultColumn.SourceColumnName),
                QueryAggregateFunction.Max => new MaxAggregation(resultColumn.ResultColumnName, resultColumn.SourceColumnName),
                _ => throw new InputArgumentException($"Unknown aggregate function {resultColumn.Function}")
            };

        private SortOrder? BuildGroupSortDirection(string groupColumnName)
        {
            var sort = _query.Sort?.FirstOrDefault(s => s.ColumnName == groupColumnName);
            return sort == null 
                ? null
                : sort.Direction == QuerySortDirection.Ascending
                    ? SortOrder.Ascending
                    : SortOrder.Descending;
        }

        protected Column GetColumn(string columnName)
        {
            var column = _table.Columns.FirstOrDefault(c => c.Name == columnName);
            return column ?? throw new InputArgumentException($"Unknown column \"{columnName}\"");
        }
    }
}
