using DatabaseBenchmark.DataSources.Parquet;
using Parquet;
using Parquet.Data;
using Parquet.Schema;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace DatabaseBenchmark.Tests.DataSources
{
    public class ParquetDataSourceTests
    {
        public ParquetDataSourceTests()
        {
        }

        [Fact]
        public async Task ReadValues()
        {
            string testFilePath = "test.parquet";

            try
            {
                await CreateTestFile(testFilePath);

                using var dataSource = new ParquetDataSource(testFilePath);

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
            finally
            {
                if (File.Exists(testFilePath))
                {
                    File.Delete(testFilePath);
                }
            }
        }

        [Fact]
        public async Task ReadListValues()
        {
            string testFilePath = "test-list.parquet";

            try
            {
                await CreateTestFileWithList(testFilePath);

                using var dataSource = new ParquetDataSource(testFilePath);

                // Row 1: Id=1, vector=[1.0, 2.0, 3.0], tags=["a", "b"]
                Assert.True(dataSource.Read());
                Assert.Equal(1, dataSource.GetValue("Id"));
                var vector1 = dataSource.GetValue("vector") as float[];
                Assert.NotNull(vector1);
                Assert.Equal(new float[] { 1.0f, 2.0f, 3.0f }, vector1);
                var tags1 = dataSource.GetValue("tags") as string[];
                Assert.NotNull(tags1);
                Assert.Equal(new string[] { "a", "b" }, tags1);

                // Row 2: Id=2, vector=[4.0, 5.0], tags=["c"]
                Assert.True(dataSource.Read());
                Assert.Equal(2, dataSource.GetValue("Id"));
                var vector2 = dataSource.GetValue("vector") as float[];
                Assert.NotNull(vector2);
                Assert.Equal(new float[] { 4.0f, 5.0f }, vector2);
                var tags2 = dataSource.GetValue("tags") as string[];
                Assert.NotNull(tags2);
                Assert.Equal(new string[] { "c" }, tags2);

                // Row 3: Id=3, vector=[6.0, 7.0, 8.0, 9.0], tags=["d", "e", "f"]
                Assert.True(dataSource.Read());
                Assert.Equal(3, dataSource.GetValue("Id"));
                var vector3 = dataSource.GetValue("vector") as float[];
                Assert.NotNull(vector3);
                Assert.Equal(new float[] { 6.0f, 7.0f, 8.0f, 9.0f }, vector3);
                var tags3 = dataSource.GetValue("tags") as string[];
                Assert.NotNull(tags3);
                Assert.Equal(new string[] { "d", "e", "f" }, tags3);

                // No more rows
                Assert.False(dataSource.Read());
            }
            finally
            {
                if (File.Exists(testFilePath))
                {
                    File.Delete(testFilePath);
                }
            }
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

        private static async Task CreateTestFileWithList(string filePath)
        {
            // Create schema with Id, a list of floats (vector), and a list of strings (tags)
            var idField = new DataField<int>("Id");
            var vectorElementField = new DataField<float>("element");
            var tagsElementField = new DataField<string>("element");
            var schema = new ParquetSchema(
                idField,
                new ListField("vector", vectorElementField),
                new ListField("tags", tagsElementField));

            using var stream = File.OpenWrite(filePath);
            using var writer = await ParquetWriter.CreateAsync(schema, stream);

            using var groupWriter = writer.CreateRowGroup();

            // Write Id column: 3 rows with values 1, 2, 3
            var idColumn = new DataColumn(idField, new int[] { 1, 2, 3 });
            await groupWriter.WriteColumnAsync(idColumn);

            // Write vector column with flattened data and repetition levels
            // Row 1: [1.0, 2.0, 3.0] -> repLevels [0, 1, 1]
            // Row 2: [4.0, 5.0]      -> repLevels [0, 1]
            // Row 3: [6.0, 7.0, 8.0, 9.0] -> repLevels [0, 1, 1, 1]
            var vectorColumn = new DataColumn(
                vectorElementField,
                new float[] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f, 9.0f },
                new int[] { 0, 1, 1, 0, 1, 0, 1, 1, 1 });
            await groupWriter.WriteColumnAsync(vectorColumn);

            // Write tags column with flattened data and repetition levels
            // Row 1: ["a", "b"]     -> repLevels [0, 1]
            // Row 2: ["c"]          -> repLevels [0]
            // Row 3: ["d", "e", "f"] -> repLevels [0, 1, 1]
            var tagsColumn = new DataColumn(
                tagsElementField,
                new string[] { "a", "b", "c", "d", "e", "f" },
                new int[] { 0, 1, 0, 0, 1, 1 });
            await groupWriter.WriteColumnAsync(tagsColumn);
        }
    }
}
