using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.AzureSearch;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.DataSources;
using DatabaseBenchmark.Tests.Utils;
using NSubstitute;
using System;
using System.Linq;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class AzureSearchInsertBuilderTests
    {
        [Fact]
        public void BuildInsertSingleRow()
        {
            var source = new DataSourceDecorator(new SampleDataSource())
                .TypedColumns(SampleInputs.Table.Columns, null)
                .DataSource;
            var reader = new DataSourceReader(source);
            var options = new InsertBuilderOptions { BatchSize = 1 };
            var optionsProvider = Substitute.For<IOptionsProvider>();
            optionsProvider.GetOptions<AzureSearchInsertOptions>().Returns(new AzureSearchInsertOptions());
            var queryBuilder = new AzureSearchInsertBuilder(SampleInputs.Table, reader, optionsProvider, options);

            var documents = queryBuilder.Build();

            //TODO: check document values
            Assert.Single(documents);
        }

        [Fact]
        public void BuildInsertMultipleRows()
        {
            var source = new DataSourceDecorator(new SampleDataSource())
                .TypedColumns(SampleInputs.Table.Columns, null)
                .DataSource;
            var reader = new DataSourceReader(source);
            var options = new InsertBuilderOptions { BatchSize = 3 };
            var optionsProvider = Substitute.For<IOptionsProvider>();
            optionsProvider.GetOptions<AzureSearchInsertOptions>().Returns(new AzureSearchInsertOptions());
            var queryBuilder = new AzureSearchInsertBuilder(SampleInputs.Table, reader, optionsProvider, options);

            var documents = queryBuilder.Build();

            //TODO: check document values
            Assert.Equal(3, documents.Count());
        }

        [Fact]
        public void BuildInsertNoMoreData()
        {
            var source = new DataSourceDecorator(new SampleDataSource())
                .TypedColumns(SampleInputs.Table.Columns, null)
                .DataSource;
            var reader = new DataSourceReader(source);
            var options = new InsertBuilderOptions { BatchSize = 3 };
            var optionsProvider = Substitute.For<IOptionsProvider>();
            optionsProvider.GetOptions<AzureSearchInsertOptions>().Returns(new AzureSearchInsertOptions());
            var queryBuilder = new AzureSearchInsertBuilder(SampleInputs.Table, reader, optionsProvider, options);

            reader.ReadDictionary(SampleInputs.Table.Columns, out var _);
            reader.ReadDictionary(SampleInputs.Table.Columns, out var _);
            reader.ReadDictionary(SampleInputs.Table.Columns, out var _);

            Assert.Throws<NoDataAvailableException>(queryBuilder.Build);
        }
    }
}
