using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Elasticsearch.Interfaces;
using DatabaseBenchmark.Model;
using Nest;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public class ElasticsearchQueryBuilder : IElasticsearchQueryBuilder
    {
        private readonly Table _table;
        private readonly Query _query;
        private readonly IRandomValueProvider _randomValueProvider;
        private readonly IRandomGenerator _randomGenerator;

        public ElasticsearchQueryBuilder(
            Table table,
            Query query,
            IRandomValueProvider randomValueProvider,
            IRandomGenerator randomGenerator)
        {
            _table = table;
            _query = query;
            _randomValueProvider = randomValueProvider;
            _randomGenerator = randomGenerator;
        }

        public SearchRequest Build()
        {
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

        private QueryContainer BuildCondition(IQueryCondition predicate)
        {
            if (predicate.RandomizeInclusion && _randomGenerator.GetRandomBoolean())
            {
                return null;
            }
            else
            {
                return predicate switch
                {
                    QueryGroupCondition groupCondition => BuildGroupCondition(groupCondition),
                    QueryPrimitiveCondition primitiveCondition => BuildPrimitiveCondition(primitiveCondition),
                    _ => throw new InputArgumentException($"Unknown predicate type \"{predicate.GetType()}\"")
                };
            }
        }

        private QueryContainer BuildGroupCondition(QueryGroupCondition predicate)
        {
            var predicates = predicate.Conditions
                .Select(p => BuildCondition(p))
                .Where(p => p != null)
                .ToArray();

            if (predicates.Any())
            {
                return predicate.Operator switch
                {
                    QueryGroupOperator.And => new BoolQuery { Must = predicates },
                    QueryGroupOperator.Or => new BoolQuery { Should = predicates },
                    QueryGroupOperator.Not => BuildNotCondition(predicates),
                    _ => throw new InputArgumentException($"Unknown group operator \"{predicate.Operator}\"")
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
                throw new InputArgumentException($"Operator NOT can have only one operand");
            }

            return new BoolQuery { MustNot = inputConditions };
        }

        private QueryContainer BuildPrimitiveCondition(QueryPrimitiveCondition predicate)
        {
            var rawValue = !predicate.RandomizeValue 
                ? predicate.Value
                : predicate.Operator == QueryPrimitiveOperator.In 
                    ? _randomValueProvider.GetRandomValueCollection(_table.Name, predicate.ColumnName, predicate.ValueRandomizationRule)
                    : _randomValueProvider.GetRandomValue(_table.Name, predicate.ColumnName, predicate.ValueRandomizationRule);

            var column = _table.Columns.First(c => c.Name == predicate.ColumnName);

            //TODO: double-check DateTime and serialization for all database types
            return predicate.Operator switch
            {
                QueryPrimitiveOperator.Equals => 
                    rawValue != null 
                        ? new TermQuery { Field = predicate.ColumnName, Value = rawValue }
                        : new BoolQuery
                        {
                            MustNot = new QueryContainer[]
                            {
                                new ExistsQuery { Field = predicate.ColumnName }
                            }
                        },
                QueryPrimitiveOperator.In => new TermQuery { Field = predicate.ColumnName, Value = rawValue },
                QueryPrimitiveOperator.NotEquals => 
                    rawValue != null 
                        ? new BoolQuery 
                        {
                            MustNot = new QueryContainer[]
                            {
                                new TermQuery { Field = predicate.ColumnName, Value = rawValue }
                            }
                        }
                        : new ExistsQuery { Field = predicate.ColumnName },
                QueryPrimitiveOperator.Lower => column.Type switch 
                    {
                        ColumnType.Integer => new LongRangeQuery { Field = predicate.ColumnName, LessThan = (long?)rawValue },
                        ColumnType.Double => new NumericRangeQuery { Field = predicate.ColumnName, LessThan = (double?)rawValue },
                        ColumnType.DateTime => new DateRangeQuery { Field = predicate.ColumnName, LessThan = (DateTime?)rawValue },
                        ColumnType.String => new TermRangeQuery { Field = predicate.ColumnName, LessThan = (string)rawValue },
                        ColumnType.Text => new TermRangeQuery { Field = predicate.ColumnName, LessThan = (string)rawValue },
                        _ => throw new InputArgumentException($"Operator Lower is not supported for column type \"{column.Type}\"")
                    },
                QueryPrimitiveOperator.LowerEquals => column.Type switch
                    {
                        ColumnType.Integer => new LongRangeQuery { Field = predicate.ColumnName, LessThanOrEqualTo = (long?)rawValue },
                        ColumnType.Double => new NumericRangeQuery { Field = predicate.ColumnName, LessThanOrEqualTo = (double?)rawValue },
                        ColumnType.DateTime => new DateRangeQuery { Field = predicate.ColumnName, LessThanOrEqualTo = (DateTime?)rawValue },
                        ColumnType.String => new TermRangeQuery { Field = predicate.ColumnName, LessThanOrEqualTo = (string)rawValue },
                        ColumnType.Text => new TermRangeQuery { Field = predicate.ColumnName, LessThanOrEqualTo = (string)rawValue },
                        _ => throw new InputArgumentException($"Operator LowerEquals is not supported for column type \"{column.Type}\"")
                    },
                QueryPrimitiveOperator.Greater => column.Type switch
                    {
                        ColumnType.Integer => new LongRangeQuery { Field = predicate.ColumnName, GreaterThan = (long?)rawValue },
                        ColumnType.Double => new NumericRangeQuery { Field = predicate.ColumnName, GreaterThan = (double?)rawValue },
                        ColumnType.DateTime => new DateRangeQuery { Field = predicate.ColumnName, GreaterThan = (DateTime?)rawValue },
                        ColumnType.String => new TermRangeQuery { Field = predicate.ColumnName, GreaterThan = (string)rawValue },
                        ColumnType.Text => new TermRangeQuery { Field = predicate.ColumnName, GreaterThan = (string)rawValue },
                        _ => throw new InputArgumentException($"Operator Greater is not supported for column type \"{column.Type}\"")
                    },
                QueryPrimitiveOperator.GreaterEquals => column.Type switch
                    {
                        ColumnType.Integer => new LongRangeQuery { Field = predicate.ColumnName, GreaterThanOrEqualTo = (long?)rawValue },
                        ColumnType.Double => new NumericRangeQuery { Field = predicate.ColumnName, GreaterThanOrEqualTo = (double?)rawValue },
                        ColumnType.DateTime => new DateRangeQuery { Field = predicate.ColumnName, GreaterThanOrEqualTo = (DateTime?)rawValue },
                        ColumnType.String => new TermRangeQuery { Field = predicate.ColumnName, GreaterThanOrEqualTo = (string)rawValue },
                        ColumnType.Text => new TermRangeQuery { Field = predicate.ColumnName, GreaterThanOrEqualTo = (string)rawValue },
                        _ => throw new InputArgumentException($"Operator GreaterEquals is not supported for column type \"{column.Type}\"")
                    },
                QueryPrimitiveOperator.Contains => new WildcardQuery { Field = predicate.ColumnName, Value = $"*{rawValue}*" },
                QueryPrimitiveOperator.StartsWith => new WildcardQuery { Field = predicate.ColumnName, Value = $"{rawValue}*"},
                _ => throw new InputArgumentException($"Unknown primitive operator \"{predicate.Operator}\"")
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

        private AggregationContainer BuildAggregation(QueryAggregateColumn resultColumn) =>
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
    }
}
