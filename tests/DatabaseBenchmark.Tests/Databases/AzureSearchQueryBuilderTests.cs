using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.AzureSearch;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Tests.Utils;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class AzureSearchQueryBuilderTests
    {
        private static Query ArrayQuery => new()
        {
            Columns = ["Category", "SubCategory"],
            Condition = new QueryPrimitiveCondition
            {
                ColumnName = "Tags",
                Operator = QueryPrimitiveOperator.Contains,
                Value = "ABC"
            }
        };

        [Fact]
        public void BuildQueryNoArguments()
        {
            var builder = new AzureSearchQueryBuilder(SampleInputs.Table, SampleInputs.NoArgumentsQuery, null, null);

            var searchOptions = builder.Build();

            Assert.Null(searchOptions.Filter);
        }

        [Fact]
        public void BuildQueryNoArgumentsDistinct()
        {
            var query = SampleInputs.NoArgumentsQuery;
            query.Distinct = true;
            var builder = new AzureSearchQueryBuilder(SampleInputs.Table, query, null, null);

            Assert.Throws<InputArgumentException>(builder.Build);
        }

        [Fact]
        public void BuildQueryDistinct()
        {
            var query = SampleInputs.NoArgumentsQuery;
            query.Distinct = true;
            query.Columns = ["Category", "SubCategory"];
            var builder = new AzureSearchQueryBuilder(SampleInputs.Table, query, null, null);

            Assert.Throws<InputArgumentException>(builder.Build);
        }

        [Fact]
        public void BuildQueryAllArguments()
        {
            var query = SampleInputs.AllArgumentsQuery;

            var builder = new AzureSearchQueryBuilder(SampleInputs.Table, query, null, null);

            Assert.Throws<InputArgumentException>(builder.Build);
        }

        [Fact]
        public void BuildQueryAllArgumentsNoAggregation()
        {
            var query = SampleInputs.AllArgumentsQuery;
            query.Aggregate = null;

            var builder = new AzureSearchQueryBuilder(SampleInputs.Table, query, null, null);

            var searchOptions = builder.Build();

            Assert.Equal("((Category eq 'ABC' or Category eq 'DEF') and SubCategory eq null and Rating ge 5 and Count eq 0 and (search.ismatchscoring('A*', 'Name') or search.ismatchscoring('B', 'Name')))",
                searchOptions.Filter);
            Assert.Equal(["Category", "SubCategory"], searchOptions.Select);
            Assert.Equal(["Category", "SubCategory"], searchOptions.OrderBy);
            Assert.Equal(10, searchOptions.Skip);
            Assert.Equal(100, searchOptions.Size);
        }

        [Fact]
        public void BuildQueryArrayColumn()
        {
            var query = SampleInputs.ArrayColumnQuery;

            var builder = new AzureSearchQueryBuilder(SampleInputs.ArrayColumnTable, query, null, null);

            Assert.Throws<InputArgumentException>(builder.Build);
        }

        [Fact]
        public void BuildQueryArrayColumnContainsOperatorOnly()
        {
            var builder = new AzureSearchQueryBuilder(SampleInputs.ArrayColumnTable, ArrayQuery, null, null);

            var searchOptions = builder.Build();

            Assert.Equal("Tags/any(item: item eq 'ABC')", searchOptions.Filter);
            Assert.Equal(["Category", "SubCategory"], searchOptions.Select);
        }

        [Theory]
        [InlineData(QueryPrimitiveOperator.Equals)]
        [InlineData(QueryPrimitiveOperator.NotEquals)]
        [InlineData(QueryPrimitiveOperator.In)]
        [InlineData(QueryPrimitiveOperator.Greater)]
        [InlineData(QueryPrimitiveOperator.GreaterEquals)]
        [InlineData(QueryPrimitiveOperator.Lower)]
        [InlineData(QueryPrimitiveOperator.LowerEquals)]
        [InlineData(QueryPrimitiveOperator.StartsWith)]
        public void BuildQueryArrayColumnUnsupportedOperator(QueryPrimitiveOperator @operator)
        {
            var query = ArrayQuery;
            //TODO: change to a query generator function that returns a query with the specified operator
            ((QueryPrimitiveCondition)query.Condition).Operator = @operator;

            var builder = new AzureSearchQueryBuilder(SampleInputs.ArrayColumnTable, query, null, null);

            Assert.Throws<InputArgumentException>(builder.Build);
        }
    }
}
