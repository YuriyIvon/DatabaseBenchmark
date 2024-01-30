using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Options;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using System;
using Xunit;

namespace DatabaseBenchmark.Tests.Generators
{
    public class DataSourceIteratorGeneratorTests
    {
        [Fact]
        public void GenerateWithNavigation()
        {
            string columnName = "Name";
            string columnValue = "Value";
            var dataSource = Substitute.For<IDataSource>();
            dataSource.Read().Returns(true);
            dataSource.GetValue( columnName).Returns(columnValue);

            var generator = new DataSourceIteratorGenerator(
                new DataSourceIteratorGeneratorOptions
                {
                    ColumnName = columnName
                },
                dataSource,
                false);

            generator.Next();
            var generatedValue = generator.Current;

            Assert.Equal(columnValue, generatedValue);
            dataSource.Received().Read();
        }

        [Fact]
        public void GenerateNoNavigation()
        {
            string columnName = "Name";
            string columnValue = "Value";
            var dataSource = Substitute.For<IDataSource>();
            dataSource.GetValue(columnName).Returns(columnValue);

            var generator = new DataSourceIteratorGenerator(
                new DataSourceIteratorGeneratorOptions
                {
                    ColumnName = columnName
                },
                dataSource,
                true);

            generator.Next();
            var generatedValue = generator.Current;

            Assert.Equal(columnValue, generatedValue);
            dataSource.DidNotReceive().Read();
        }
    }
}
