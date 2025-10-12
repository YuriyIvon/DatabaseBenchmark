using Azure.Search.Documents;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.AzureSearch.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.AzureSearch
{
    public class AzureSearchQueryBuilder : IAzureSearchQueryBuilder
    {
        private static readonly QueryPrimitiveOperator[] AllowedArrayOperators =
        [
            QueryPrimitiveOperator.Contains
        ];

        private readonly Table _table;
        private readonly Query _query;
        private readonly IRandomValueProvider _randomValueProvider;
        private readonly IRandomPrimitives _randomPrimitives;
        private readonly IValueFormatter _valueFormatter = new AzureSearchValueFormatter();

        public AzureSearchQueryBuilder(
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

        public SearchOptions Build()
        {
            _randomValueProvider?.Next();

            if (_query.Distinct)
            {
                throw new InputArgumentException("Distinct queries are not yet supported for Azure Search");
            }

            var options = new SearchOptions();

            if (_query.Columns != null)
            {
                foreach (var column in _query.Columns)
                {
                    options.Select.Add(column);
                }
            }

            if (_query.Condition != null)
            {
                options.Filter = BuildCondition(_query.Condition);
            }

            if (_query.Aggregate != null)
            {
                throw new InputArgumentException("Aggregation queries are not yet supported for Azure Search");
            }

            if (_query.Sort != null)
            {
                foreach (var sort in _query.Sort)
                {
                    options.OrderBy.Add(sort.Direction == QuerySortDirection.Ascending 
                        ? sort.ColumnName 
                        : $"{sort.ColumnName} desc");
                }
            }

            if (_query.Skip > 0)
            {
                options.Skip = _query.Skip;
            }

            if (_query.Take > 0)
            {
                options.Size = _query.Take;
            }

            return options;
        }

        private string BuildCondition(IQueryCondition condition)
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

        private string BuildGroupCondition(QueryGroupCondition condition)
        {
            var conditions = condition.Conditions
                .Select(BuildCondition)
                .Where(p => !string.IsNullOrEmpty(p))
                .ToArray();

            if (conditions.Any())
            {
                return condition.Operator switch
                {
                    QueryGroupOperator.And => $"({string.Join(" and ", conditions)})",
                    QueryGroupOperator.Or => $"({string.Join(" or ", conditions)})",
                    QueryGroupOperator.Not => BuildNotCondition(conditions),
                    _ => throw new InputArgumentException($"Unknown group operator \"{condition.Operator}\"")
                };
            }
            else
            {
                return null;
            }
        }

        private string BuildNotCondition(string[] inputConditions)
        {
            if (inputConditions.Length > 1)
            {
                throw new InputArgumentException($"Operator \"Not\" can have only one operand");
            }

            return $"not ({inputConditions.First()})";
        }

        private string BuildInCondition(string columnName, object value)
        {
            if (value is IEnumerable<object> collectionValue)
            {
                return BuildGroupCondition(
                    new QueryGroupCondition
                    {
                        Operator = QueryGroupOperator.Or,
                        Conditions = collectionValue.Select(v => 
                            new QueryPrimitiveCondition
                            {
                                ColumnName = columnName,
                                Operator = QueryPrimitiveOperator.Equals,
                                Value = v,
                                RandomizeInclusion = false,
                                RandomizeValue = false
                            }
                        ).ToArray()
                    });
            }

            throw new InputArgumentException("An \"In\" operator parameter must be a collection");
        }

        private string BuildPrimitiveCondition(QueryPrimitiveCondition condition)
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

            string valueString = _valueFormatter.Format(rawValue);

            if (column.Array)
            {
                return condition.Operator switch
                {
                    QueryPrimitiveOperator.Contains => 
                        rawValue != null 
                            ? $"{condition.ColumnName}/any(item: item eq {valueString})" 
                            : throw new InputArgumentException("Cannot search for NULL values in arrays with \"Contains\" operator"),
                    _ => throw new InputArgumentException($"Primitive operator \"{condition.Operator}\" is not supported for array columns")
                };
            }
            else
            {
                return condition.Operator switch
                {
                    QueryPrimitiveOperator.Equals => 
                        rawValue != null 
                            ? $"{condition.ColumnName} eq {valueString}" 
                            : $"{condition.ColumnName} eq null",
                    QueryPrimitiveOperator.NotEquals => 
                        rawValue != null 
                            ? $"{condition.ColumnName} ne {valueString}" 
                            : $"{condition.ColumnName} ne null",
                    QueryPrimitiveOperator.In => BuildInCondition(condition.ColumnName, rawValue),
                    QueryPrimitiveOperator.Lower => $"{condition.ColumnName} lt {valueString}",
                    QueryPrimitiveOperator.LowerEquals => $"{condition.ColumnName} le {valueString}",
                    QueryPrimitiveOperator.Greater => $"{condition.ColumnName} gt {valueString}",
                    QueryPrimitiveOperator.GreaterEquals => $"{condition.ColumnName} ge {valueString}",
                    QueryPrimitiveOperator.Contains => 
                        column.Type == ColumnType.String || column.Type == ColumnType.Text 
                            ? $"search.ismatchscoring('{rawValue}', '{condition.ColumnName}')" 
                            : throw new InputArgumentException("Operator \"Contains\" is only supported for string columns"),
                    QueryPrimitiveOperator.StartsWith => 
                        column.Type == ColumnType.String || column.Type == ColumnType.Text 
                            ? $"search.ismatchscoring('{rawValue}*', '{condition.ColumnName}')" 
                            : throw new InputArgumentException("Operator \"StartsWith\" is only supported for string columns"),
                    _ => throw new InputArgumentException($"Unknown primitive operator \"{condition.Operator}\"")
                };
            }
        }

        protected Column GetColumn(string columnName)
        {
            var column = _table.Columns.FirstOrDefault(c => c.Name == columnName);
            return column ?? throw new InputArgumentException($"Unknown column \"{columnName}\"");
        }
    }
}