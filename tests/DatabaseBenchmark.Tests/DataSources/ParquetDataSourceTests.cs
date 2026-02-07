using DatabaseBenchmark.DataSources.Parquet;
using Parquet;
using Parquet.Data;
using Parquet.Schema;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace DatabaseBenchmark.Tests.DataSources
{
    public class ParquetDataSourceTests : IDisposable
    {
        private readonly string _testFilePath = "test.parquet";

        public ParquetDataSourceTests()
        {
        }

        public void Dispose()
        {
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        [Fact]
        public async Task ReadValues()
        {
            await CreateTestFile(_testFilePath);

            using var dataSource = new ParquetDataSource(_testFilePath);

            // Row 1 from row group 1
            Assert.True(dataSource.Read());
            Assert.Equal(1, dataSource.GetValue("ArchiveId"));
            Assert.Equal("One", dataSource.GetValue("Name"));
            Assert.Equal(10.1, dataSource.GetValue("Price"));

            // Row 2 from row group 1
            Assert.True(dataSource.Read());
            Assert.Equal(2, dataSource.GetValue("ArchiveId"));
            Assert.Equal("Two", dataSource.GetValue("Name"));
            Assert.Equal(20.2, dataSource.GetValue("Price"));

            // Row 1 from row group 2 (same data pattern)
            Assert.True(dataSource.Read());
            Assert.Equal(1, dataSource.GetValue("ArchiveId"));
            Assert.Equal("One", dataSource.GetValue("Name"));
            Assert.Equal(10.1, dataSource.GetValue("Price"));

            // Row 2 from row group 2
            Assert.True(dataSource.Read());
            Assert.Equal(2, dataSource.GetValue("ArchiveId"));
            Assert.Equal("Two", dataSource.GetValue("Name"));
            Assert.Equal(20.2, dataSource.GetValue("Price"));

            // No more rows
            Assert.False(dataSource.Read());
        }

        private static async Task CreateTestFile(string filePath)
        {
            var schema = new ParquetSchema(
                new DataField<int>("ArchiveId"),
                new DataField<string>("Name"),
                new DataField<double>("Price"));

            var column1 = new DataColumn(schema.DataFields[0], new int[] { 1, 2 });
            var column2 = new DataColumn(schema.DataFields[1], new string[] { "One", "Two" });
            var column3 = new DataColumn(schema.DataFields[2], new double[] { 10.1, 20.2 });

            using var stream = File.OpenWrite(filePath);
            using var writer = await ParquetWriter.CreateAsync(schema, stream);

            // Write first row group
            using (var groupWriter = writer.CreateRowGroup())
            {
                await groupWriter.WriteColumnAsync(column1);
                await groupWriter.WriteColumnAsync(column2);
                await groupWriter.WriteColumnAsync(column3);
            }

            // Write second row group with same data
            using (var groupWriter = writer.CreateRowGroup())
            {
                await groupWriter.WriteColumnAsync(column1);
                await groupWriter.WriteColumnAsync(column2);
                await groupWriter.WriteColumnAsync(column3);
            }
        }
    }
}
