using DatabaseBenchmark.Common;
using DatabaseBenchmark.DataSources.Generator;
using System;
using Xunit;

namespace DatabaseBenchmark.Tests.DataSources
{
    public class GeneratorDataSourceTests
    {
        [Fact]
        public void GenerateValues()
        {
            var dataSource = new GeneratorDataSource("DataSources/GeneratorDataSourceOptions.json", null, null);
            int id = 1;
            var createdAt = new DateTime(2020, 1, 1, 13, 0, 0);

            for (int i = 0; i < 10; i++)
            {
                dataSource.Read();

                var generatedId = dataSource.GetValue("Id");
                var generatedCreatedAt = dataSource.GetValue("CreatedAt");

                Assert.IsType<int>(generatedId);
                Assert.IsType<DateTime>(generatedCreatedAt);
                Assert.Equal(id, (int)generatedId);
                Assert.Equal(createdAt, (DateTime)generatedCreatedAt);

                id++;
                createdAt = createdAt.AddDays(1);
            }
        }

        [Fact]
        public void GenerateValuesNoMaxRows()
        {
            var dataSource = new GeneratorDataSource("DataSources/UnboundedGeneratorDataSourceOptions.json", null, null);
            
            Assert.Throws<InputArgumentException>(() => dataSource.SetMaxRows(0));
        }
    }
}
