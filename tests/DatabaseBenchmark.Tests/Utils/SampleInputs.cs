using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Tests.Utils
{
    public static class SampleInputs
    {
        public static Table Table { get; } = new()
        {
            Name = "Sample",
            Columns = new Column[]
            {
                new Column
                {
                    Name = "Id",
                    Type = ColumnType.Integer,
                    Queryable = true,
                    Nullable = false
                },
                new Column
                {
                    Name = "Category",
                    Type = ColumnType.String,
                    Queryable = true,
                    Nullable = true
                },
                new Column
                {
                    Name = "SubCategory",
                    Type = ColumnType.String,
                    Queryable = true,
                    Nullable = true
                },
                new Column
                {
                    Name = "Name",
                    Type = ColumnType.String,
                    Queryable = true,
                    Nullable = false
                },
                new Column
                {
                    Name = "CreatedAt",
                    Type = ColumnType.DateTime,
                    Queryable = true,
                    Nullable = false
                },
                new Column
                {
                    Name = "Price",
                    Type = ColumnType.Double,
                    Queryable = false,
                    Nullable = false
                },
                new Column
                {
                    Name = "Count",
                    Type = ColumnType.Integer,
                    Queryable = false,
                    Nullable = false
                }
            }
        };

        public static Query NoArgumentsQuery { get; } = new Query();

        public static Query AllArgumentsQuery { get; } = new Query
        {
            Columns = new string[] { "Category", "SubCategory" },
            Condition = new QueryGroupCondition
            {
                Operator = QueryGroupOperator.And,
                Conditions = new IQueryCondition[]
                {
                    new QueryPrimitiveCondition
                    {
                        ColumnName = "Category",
                        Operator = QueryPrimitiveOperator.Equals,
                        Value = "ABC"
                    },
                    new QueryPrimitiveCondition
                    {
                        ColumnName = "SubCategory",
                        Operator = QueryPrimitiveOperator.Equals,
                        Value = null
                    }
                }
            },
            Aggregate = new QueryAggregate
            {
                GroupColumnNames = new string[] { "Category", "SubCategory" },
                ResultColumns = new QueryAggregateColumn[]
                {
                    new QueryAggregateColumn
                    {
                        SourceColumnName = "Price",
                        Function = QueryAggregateFunction.Sum,
                        ResultColumnName = "TotalPrice"
                    }
                }
            },
            Sort = new QuerySort[]
            {
                new QuerySort
                {
                    ColumnName = "Category",
                    Direction = QuerySortDirection.Ascending,
                },
                new QuerySort
                {
                    ColumnName = "SubCategory",
                    Direction = QuerySortDirection.Ascending,
                }
            },
            Skip = 10,
            Take = 100
        };

        public static RawQuery RawSqlQuery { get; } = new RawQuery
        {
            Text = "SELECT * FROM Sample WHERE Category = ${category}",
            Parameters = new RawQueryParameter[]
            {
                new RawQueryParameter
                {
                    Name = "category",
                    Type = ColumnType.String,
                    Value = "ABC"
                }
            }
        };
    }
}
