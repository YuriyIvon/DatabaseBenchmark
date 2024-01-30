using DatabaseBenchmark.DataSources.Generator;
using System;
using Xunit;

namespace DatabaseBenchmark.Tests.DataSources
{
    public class GeneratorDataSourceTests
    {
        private readonly GeneratorDataSource _dataSource;

        public GeneratorDataSourceTests()
        {
            _dataSource = new GeneratorDataSource("DataSources/GeneratorDataSourceOptions.json", null, null);
        }

        [Fact]
        public void GenerateValues()
        {
            int id = 1;
            var createdAt = new DateTime(2020, 1, 1, 13, 0, 0);

            for (int i = 0; i < 10; i++)
            {
                _dataSource.Read();

                var generatedId = _dataSource.GetValue("Id");
                var generatedCreatedAt = _dataSource.GetValue("CreatedAt");

                Assert.IsType<int>(generatedId);
                Assert.IsType<DateTime>(generatedCreatedAt);
                Assert.Equal(id, (int)generatedId);
                Assert.Equal(createdAt, (DateTime)generatedCreatedAt);

                id++;
                createdAt = createdAt.AddDays(1);
            }
        }
    }
}
