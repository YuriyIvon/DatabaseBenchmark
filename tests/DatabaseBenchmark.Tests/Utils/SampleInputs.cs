using DatabaseBenchmark.Model;
using System.Linq;

namespace DatabaseBenchmark.Tests.Utils
{
    public static class SampleInputs
    {
        public static Table Table => new()
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
                    Name = "Rating",
                    Type = ColumnType.Double,
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

        public static Query NoArgumentsQuery => new();

        public static Query SpecificFieldsQuery => new()
        {
            Columns = new string[]
            {
                "Id",
                "Category",
                "Name",
                "CreatedAt",
                "Rating",
                "Price"
            }
        };

        public static Query AllArgumentsQuery => new()
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
                    },
                    new QueryPrimitiveCondition
                    {
                        ColumnName = "Rating",
                        Operator = QueryPrimitiveOperator.GreaterEquals,
                        Value = 5.0
                    },
                    new QueryGroupCondition
                    {
                        Operator = QueryGroupOperator.Or,
                        Conditions = new QueryPrimitiveCondition[]
                        {
                            new QueryPrimitiveCondition
                            {
                                ColumnName = "Name",
                                Operator = QueryPrimitiveOperator.StartsWith,
                                Value = "A"
                            },
                            new QueryPrimitiveCondition
                            {
                                ColumnName = "Name",
                                Operator = QueryPrimitiveOperator.Contains,
                                Value = "B"
                            }
                        }
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

        public static Query AllArgumentsQueryRandomizeInclusionAll
        {
            get
            {
                var query = AllArgumentsQuery;
                var topCondition = (QueryGroupCondition)query.Condition;
                foreach (var condition in topCondition.Conditions)
                {
                    //Don't want to have the setter in the interface
                    switch (condition)
                    {
                        case QueryGroupCondition group:
                            group.RandomizeInclusion = true;
                            break;
                        case QueryPrimitiveCondition primitive:
                            primitive.RandomizeInclusion = true;
                            break;
                    }
                }

                return query;
            }
        }

        public static Query AllArgumentsQueryRandomizeInclusionPartial
        {
            get
            {
                var query = AllArgumentsQuery;
                var topCondition = (QueryGroupCondition)query.Condition;
                topCondition.Conditions.Cast<QueryPrimitiveCondition>().First().RandomizeInclusion = true;

                return query;
            }
        }

        public static RawQuery RawSqlQuery => new()
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
