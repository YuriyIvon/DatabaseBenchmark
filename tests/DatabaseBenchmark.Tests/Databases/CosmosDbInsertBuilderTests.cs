﻿using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.CosmosDb;
using DatabaseBenchmark.Tests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class CosmosDbInsertBuilderTests
    {
        [Fact]
        public void BuildInsertSingleRow()
        {
            var source = new SampleDataSource();
            var reader = new DataSourceReader(source);
            var options = new InsertBuilderOptions { BatchSize = 1 };
            var queryBuilder = new CosmosDbInsertBuilder(SampleInputs.Table, reader, options);

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
            var queryBuilder = new CosmosDbInsertBuilder(SampleInputs.Table, reader, options);

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
            var queryBuilder = new CosmosDbInsertBuilder(SampleInputs.Table, reader, options);

            reader.ReadDictionary(SampleInputs.Table.Columns, out var _);
            reader.ReadDictionary(SampleInputs.Table.Columns, out var _);
            reader.ReadDictionary(SampleInputs.Table.Columns, out var _);
            
            Assert.Throws<NoDataAvailableException>(queryBuilder.Build);
        }
    }
}
