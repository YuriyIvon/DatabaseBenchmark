using DatabaseBenchmark.Databases.AzureSearch;
using DatabaseBenchmark.Tests.Utils;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class AzureSearchRawQueryBuilderTests
    {
        [Fact]
        public void BuildParameterizedQuery()
        {
            var query = SampleInputs.RawAzureSearchQuery;
            var builder = new AzureSearchRawQueryBuilder(query, null);

            var searchOptions = builder.Build();

            Assert.Equal(@"Category eq 'ABC' and createdDate ge 2020-01-02T03:04:05.0000000Z and price le 25.5 and available eq true", searchOptions.Filter);
            Assert.Empty(searchOptions.Select);
            Assert.Equal(["createdDate"], searchOptions.OrderBy);
            Assert.Equal(2, searchOptions.Skip);
            Assert.Equal(20, searchOptions.Size);
        }

        [Fact]
        public void BuildInlineParameterizedQuery()
        {
            var query = SampleInputs.RawAzureSearchInlineQuery;
            var builder = new AzureSearchRawQueryBuilder(query, null);

            var searchOptions = builder.Build();

            Assert.Equal(@"Category eq 'ABC' and createdDate ge 2020-01-02T03:04:05.0000000 and price le 25.5 and available eq true", searchOptions.Filter);
            Assert.Empty(searchOptions.Select);
            Assert.Equal(["createdDate"], searchOptions.OrderBy);
            Assert.Equal(2, searchOptions.Skip);
            Assert.Equal(20, searchOptions.Size);
        }

        [Fact]
        public void BuildArrayQuery()
        {
            var query = SampleInputs.RawAzureSearchArrayQuery;
            var builder = new AzureSearchRawQueryBuilder(query, null);

            var searchOptions = builder.Build();

            Assert.Equal(@"Tags/any(item: item eq 'One')", searchOptions.Filter);
            Assert.Empty(searchOptions.Select);
            Assert.Empty(searchOptions.OrderBy);
            Assert.Null(searchOptions.Skip);
            Assert.Null(searchOptions.Size);
        }
    }
}
