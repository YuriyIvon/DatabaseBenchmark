using DatabaseBenchmark.DataSources.Decorators;
using DatabaseBenchmark.DataSources.Interfaces;
using NSubstitute;
using Xunit;

namespace DatabaseBenchmark.Tests.DataSources
{
    public class DataSourceMappingDecoratorTests
    {
        [Fact]
        public void ReadRow()
        {
            var dataSource = Substitute.For<IDataSource>();

            var decorator = new DataSourceMappingDecorator(dataSource, new ColumnMappingCollection
            {
                Columns = new[]
                {
                    new ColumnMapping
                    {
                        SourceColumnName = "Source",
                        TableColumnName = "Table"
                    }
                }
            });

            decorator.Read();
            decorator.GetValue("Table");
            decorator.Dispose();

            dataSource.Received().GetValue("Source");
            dataSource.Received().Dispose();
        }
    }
}
