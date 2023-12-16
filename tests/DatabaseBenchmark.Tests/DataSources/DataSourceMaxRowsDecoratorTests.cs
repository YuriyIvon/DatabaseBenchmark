using DatabaseBenchmark.DataSources.Decorators;
using DatabaseBenchmark.DataSources.Interfaces;
using NSubstitute;
using Xunit;

namespace DatabaseBenchmark.Tests.DataSources
{
    public class DataSourceMaxRowsDecoratorTests
    {
        [Fact]
        public void ReadAllRows()
        {
            int referenceCount = 20;
            int maxCount = 100;

            var dataSource = Substitute.For<IDataSource>();
            dataSource.Read().Returns(true);
            var decorator = new DataSourceMaxRowsDecorator(dataSource, referenceCount);

            int count = 0;
            for (; count < maxCount && decorator.Read(); count++) { }

            Assert.Equal(referenceCount, count);
        }
    }
}
