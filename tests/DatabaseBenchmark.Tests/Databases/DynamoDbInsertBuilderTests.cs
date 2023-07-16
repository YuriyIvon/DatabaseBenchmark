using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.DynamoDb;
using DatabaseBenchmark.Tests.Utils;
using System.Linq;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class DynamoDbInsertBuilderTests
    {
        [Fact]
        public void BuildInsertSingleRow()
        {
            var source = new SampleDataSource();
            var reader = new DataSourceReader(source);
            var options = new InsertBuilderOptions { BatchSize = 1 };
            var queryBuilder = new DynamoDbInsertBuilder(SampleInputs.Table, reader, options);

            var documents = queryBuilder.Build();

            //TODO: check document values
            Assert.Single(documents);
        }

        [Fact]
        public void BuildInsertMultipleRows()
        {
            var source = new SampleDataSource();
            var reader = new DataSourceReader(source);
            var options = new InsertBuilderOptions { BatchSize = 3 };
            var queryBuilder = new DynamoDbInsertBuilder(SampleInputs.Table, reader, options);

            var documents = queryBuilder.Build();

            //TODO: check document values
            Assert.Equal(3, documents.Count());
        }

        [Fact]
        public void BuildInsertNoMoreData()
        {
            var source = new SampleDataSource();
            var reader = new DataSourceReader(source);
            var options = new InsertBuilderOptions { BatchSize = 3 };
            var queryBuilder = new DynamoDbInsertBuilder(SampleInputs.Table, reader, options);

            reader.ReadDictionary(SampleInputs.Table.Columns, out var _);
            reader.ReadDictionary(SampleInputs.Table.Columns, out var _);
            reader.ReadDictionary(SampleInputs.Table.Columns, out var _);
            var documents = queryBuilder.Build();

            Assert.Empty(documents);
        }
    }
}
