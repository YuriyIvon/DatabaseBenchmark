using DatabaseBenchmark.Model;
using System;
using System.Linq;

namespace DatabaseBenchmark.Tests.Utils
{
    public static class SampleInputs
    {
        public static Table Table => new()
        {
            Name = "Sample",
            Columns =
            [
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
            ]
        };

        public static Table ArrayColumnTable => new()
        {
            Name = "Sample",
            Columns =
            [
                .. Table.Columns,
                new Column
                {
                    Name = "Tags",
                    Type = ColumnType.String,
                    Queryable = true,
                    Nullable = true,
                    Array = true
                }
            ]
        };

        public static Query NoArgumentsQuery => new();

        public static Query SpecificFieldsQuery => new()
        {
            Columns =
            [
                "Id",
                "Category",
                "Name",
                "CreatedAt",
                "Rating",
                "Price"
            ]
        };

        public static Query AllArgumentsQuery => new()
        {
            Columns = [ "Category", "SubCategory" ],
            Condition = new QueryGroupCondition
            {
                Operator = QueryGroupOperator.And,
                Conditions =
                [
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
                        Conditions =
                        [
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
                        ]
                    }
                ]
            },
            Aggregate = new QueryAggregate
            {
                GroupColumnNames = ["Category", "SubCategory"],
                ResultColumns =
                [
                    new QueryAggregateColumn
                    {
                        SourceColumnName = "Price",
                        Function = QueryAggregateFunction.Sum,
                        ResultColumnName = "TotalPrice"
                    }
                ]
            },
            Sort =
            [
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
            ],
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

        public static Query ArrayColumnQuery => new()
        {
            Columns = ["Category", "SubCategory"],
            Condition = new QueryGroupCondition
            {
                Operator = QueryGroupOperator.Or,
                Conditions =
                [
                    new QueryPrimitiveCondition
                    {
                        ColumnName = "Tags",
                        Operator = QueryPrimitiveOperator.Contains,
                        Value = "ABC"
                    },
                    new QueryPrimitiveCondition
                    {
                        ColumnName = "Tags",
                        Operator = QueryPrimitiveOperator.Equals,
                        Value = new object[] { "A", "B", "C" }
                    }
                ]
            }
        };

        public static Query ArrayColumnNullQuery => new()
        {
            Columns = ["Category", "SubCategory"],
            Condition = new QueryPrimitiveCondition
            {
                ColumnName = "Tags",
                Operator = QueryPrimitiveOperator.Equals,
                Value = null
            }
        };

        public static RawQuery RawSqlQuery => new()
        {
            Text = "SELECT * FROM Sample WHERE Category = ${category} AND CreatedDate >= ${minDate} AND Price <= ${maxPrice} AND Available = ${available}",
            Parameters = RawQueryParameters
        };

        public static RawQuery RawSqlInlineQuery => new()
        {
            Text = "SELECT * FROM Sample WHERE Category = '${category}' AND CreatedDate >= '${minDate}' AND Price <= ${maxPrice}",
            Parameters = RawQueryInlineParameters
        };

        public static RawQuery RawElasticsearchQuery => new()
        {
            Text = @"{""query"":{""bool"":{""must"":[{""term"":{""category"":{""value"":${category}}}},{""range"":{""createdDate"":{""gte"":${minDate}}}},{""range"":{""price"":{""lte"":${maxPrice}}}},{""term"":{""available"":{""value"":${available}}}}]}}}",
            Parameters = RawQueryParameters
        };

        public static RawQuery RawElasticsearchInlineQuery => new()
        {
            Text = @"{""query"":{""bool"":{""must"":[{""term"":{""category"":{""value"":""${category}""}}},{""range"":{""createdDate"":{""gte"":""${minDate}""}}},{""range"":{""price"":{""lte"":${maxPrice}}}},{""term"":{""available"":{""value"":${available}}}}]}}}",
            Parameters = RawQueryInlineParameters
        };

        public static RawQuery RawMongoDbQuery => new()
        {
            Text = @"[{ ""$match"" : { ""$and"" : [{ ""category"" : ${category} }, { ""createdDate"" : { ""$gte"" : ${minDate} } }, { ""price"" : { ""$lte"" : ${maxPrice} } }, { ""available"" : ${available} }] } }]",
            Parameters = RawQueryParameters
        };

        public static RawQuery RawMongoDbInlineQuery => new()
        {
            Text = @"[{ ""$match"" : { ""$and"" : [{ ""category"" : ""${category}"" }, { ""createdDate"" : { ""$gte"" : ""${minDate}"" } }, { ""price"" : { ""$lte"" : ${maxPrice} } }, { ""available"" : ${available} }] } }]",
            Parameters = RawQueryInlineParameters
        };

        public static RawQueryParameter[] RawQueryParameters =>
        [
            new RawQueryParameter
            {
                Name = "category",
                Type = ColumnType.String,
                Value = "ABC"
            },
            new RawQueryParameter
            {
                Name = "minDate",
                Type = ColumnType.DateTime,
                Value = new DateTime(2020, 1, 2, 3, 4, 5)
            },
            new RawQueryParameter
            {
                Name = "maxPrice",
                Type = ColumnType.Double,
                Value = 25.5
            },
            new RawQueryParameter
            {
                Name = "available",
                Type = ColumnType.Boolean,
                Value = true
            }
        ];

        public static RawQueryParameter[] RawQueryInlineParameters =>
        [
            new RawQueryParameter
            {
                Name = "category",
                Type = ColumnType.String,
                Value = "ABC",
                Inline = true
            },
            new RawQueryParameter
            {
                Name = "minDate",
                Type = ColumnType.DateTime,
                Value = new DateTime(2020, 1, 2, 3, 4, 5),
                Inline = true
            },
            new RawQueryParameter
            {
                Name = "maxPrice",
                Type = ColumnType.Double,
                Value = 25.5,
                Inline = true
            },
            new RawQueryParameter
            {
                Name = "available",
                Type = ColumnType.Boolean,
                Value = true,
                Inline = true
            }
        ];
    }
}
