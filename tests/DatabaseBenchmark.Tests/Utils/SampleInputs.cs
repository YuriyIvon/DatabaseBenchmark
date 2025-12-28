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
                    Queryable = true,
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

        public static Table VectorColumnTable => new()
        {
            Name = "Sample",
            Columns =
            [
                .. Table.Columns,
                new Column
                {
                    Name = "TextVector",
                    Type = ColumnType.Vector,
                    Queryable = true,
                    Nullable = false
                },
                new Column
                {
                    Name = "ImageVector",
                    Type = ColumnType.Vector,
                    Queryable = true,
                    Nullable = false
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
            Columns = ["Category", "SubCategory"],
            Condition = new QueryGroupCondition
            {
                Operator = QueryGroupOperator.And,
                Conditions =
                [
                    new QueryPrimitiveCondition
                    {
                        ColumnName = "Category",
                        Operator = QueryPrimitiveOperator.In,
                        Value = new string[] { "ABC", "DEF" }
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
                    new QueryPrimitiveCondition
                    {
                        ColumnName = "Count",
                        Operator = QueryPrimitiveOperator.Equals,
                        Value = 0
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
            Text = "SELECT * FROM Sample WHERE Category = ${category} AND CreatedDate >= ${minDate} AND Price <= ${maxPrice} AND Available = ${available} AND UserId <> ${userId}",
            Parameters = RawQueryParameters
        };

        public static RawQuery RawSqlInlineQuery => new()
        {
            Text = "SELECT * FROM Sample WHERE Category = '${category}' AND CreatedDate >= '${minDate}' AND Price <= ${maxPrice}",
            Parameters = RawQueryInlineParameters
        };

        public static RawQuery RawSqlArrayQuery => new()
        {
            Text = "SELECT * FROM Sample WHERE Tags = ${tags}",
            Parameters = RawQueryArrayParameters
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
            Text = @"[{ ""$match"" : { ""$and"" : [{ ""category"" : ${category} }, { ""createdDate"" : { ""$gte"" : ${minDate} } }, { ""price"" : { ""$lte"" : ${maxPrice} } }, { ""available"" : ${available} }, { ""userId"" : { ""$ne"" : ${userId} } }] } }]",
            Parameters = RawQueryParameters
        };

        public static RawQuery RawMongoDbInlineQuery => new()
        {
            Text = @"[{ ""$match"" : { ""$and"" : [{ ""category"" : ""${category}"" }, { ""createdDate"" : { ""$gte"" : ""${minDate}"" } }, { ""price"" : { ""$lte"" : ${maxPrice} } }, { ""available"" : ${available} }] } }]",
            Parameters = RawQueryInlineParameters
        };

        public static RawQuery RawMongoDbArrayQuery => new()
        {
            Text = @"[{ ""$match"" : { ""Tags"" : ${tags} } }]",
            Parameters = RawQueryArrayParameters
        };

        public static RawQuery RawAzureSearchQuery => new()
        {
            Text = @"{""Filter"": ""Category eq ${category} and createdDate ge ${minDate} and price le ${maxPrice} and available eq ${available}"", ""OrderBy"": [""createdDate""], ""Skip"": 2, ""Size"": 20}",
            Parameters = RawQueryParameters
        };

        public static RawQuery RawAzureSearchInlineQuery => new()
        {
            Text = @"{""Filter"": ""Category eq '${category}' and createdDate ge ${minDate} and price le ${maxPrice} and available eq ${available}"", ""OrderBy"": [""createdDate""], ""Skip"": 2, ""Size"": 20}",
            Parameters = RawQueryInlineParameters
        };

        public static RawQuery RawAzureSearchArrayQuery => new()
        {
            Text = @"{""Filter"": ""Tags/any(item: item eq ${tag})""}",
            Parameters = [
                new RawQueryParameter
                {
                    Name = "tag",
                    Type = ColumnType.String,
                    Value = "One"
                }
            ]
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
            },
            new RawQueryParameter
            {
                Name = "userId",
                Type = ColumnType.Guid,
                Value = "d5a611c6-aa28-4842-8643-6a58e2f8123e"
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

        public static RawQueryParameter[] RawQueryArrayParameters =>
        [
            new RawQueryParameter
            {
                Name = "tags",
                Type = ColumnType.String,
                Array = true,
                Value = new object[] { "One", "Two" }
            }
        ];

        public static Query SingleVectorRankingQuery => new()
        {
            Columns = ["Id", "Name"],
            Ranking = new QueryRanking
            {
                FusionStrategy = RankingQueryFusionStrategy.ReciprocalRankFusion,
                Queries =
                [
                    new VectorRankingQuery
                    {
                        ColumnName = "TextVector",
                        Vector = [0.1f, 0.2f, 0.3f, 0.4f],
                        Limit = 10,
                        Weight = 1.0f
                    }
                ]
            },
            Take = 10
        };

        public static Query MultipleVectorRankingQueryRRF => new()
        {
            Columns = ["Id", "Name"],
            Ranking = new QueryRanking
            {
                FusionStrategy = RankingQueryFusionStrategy.ReciprocalRankFusion,
                Queries =
                [
                    new VectorRankingQuery
                    {
                        ColumnName = "TextVector",
                        Vector = [0.1f, 0.2f, 0.3f, 0.4f],
                        Limit = 10,
                        Weight = 1.0f
                    },
                    new VectorRankingQuery
                    {
                        ColumnName = "ImageVector",
                        Vector = [0.5f, 0.6f, 0.7f, 0.8f],
                        Limit = 10,
                        Weight = 0.5f
                    }
                ]
            },
            Take = 10
        };

        public static Query MultipleVectorRankingQueryWeighted => new()
        {
            Columns = ["Id", "Name"],
            Ranking = new QueryRanking
            {
                FusionStrategy = RankingQueryFusionStrategy.WeightedAverage,
                Queries =
                [
                    new VectorRankingQuery
                    {
                        ColumnName = "TextVector",
                        Vector = [0.1f, 0.2f, 0.3f, 0.4f],
                        Limit = 10,
                        Weight = 0.7f
                    },
                    new VectorRankingQuery
                    {
                        ColumnName = "ImageVector",
                        Vector = [0.5f, 0.6f, 0.7f, 0.8f],
                        Limit = 10,
                        Weight = 0.3f
                    }
                ]
            },
            Take = 10
        };
    }
}
