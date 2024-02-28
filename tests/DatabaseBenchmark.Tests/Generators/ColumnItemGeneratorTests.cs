using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Options;
using DatabaseBenchmark.Model;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DatabaseBenchmark.Tests.Generators
{
    public class ColumnItemGeneratorTests
    {
        private readonly IDatabase _database;
        private readonly ColumnItemGeneratorOptions _options = new()
        {
            TableName = "Test",
            ColumnName = "Id",
            ColumnType = ColumnType.Integer
        };

        public ColumnItemGeneratorTests()
        {
            _database = Substitute.For<IDatabase>();
            var executorFactory = Substitute.For<IQueryExecutorFactory>();
            var executor = Substitute.For<IQueryExecutor>();
            var preparedQuery = Substitute.For<IPreparedQuery>();
            _database.CreateQueryExecutorFactory(Arg.Any<Table>(), Arg.Any<Query>()).Returns(executorFactory);
            executorFactory.Create().Returns(executor);
            executor.Prepare().Returns(preparedQuery);
            preparedQuery.Results.Returns(new TestQueryResults());
        }

        [Fact]
        public void GenerateValue()
        {
            var generator = new ColumnItemGenerator(_options, _database);

            generator.Next();
            var value = generator.Current;

            Assert.Contains((int)value, TestQueryResults.Values);
            _database.CreateQueryExecutorFactory(Arg.Any<Table>(), Arg.Any<Query>()).Received(1);
        }

        [Fact]
        public void GenerateCollection()
        {
            var generator = new ColumnItemGenerator(_options, _database);

            generator.NextCollection(3);
            var collection = generator.CurrentCollection;

            Assert.Equal(3, collection.Count());
            Assert.True(collection.All(i => TestQueryResults.Values.Contains((int)i)));
            _database.CreateQueryExecutorFactory(Arg.Any<Table>(), Arg.Any<Query>()).Received(1);
        }

        [Fact]
        public void GenerateValueWithMaxSourceRows()
        {
            _options.MaxSourceRows = 1;

            var generator = new ColumnItemGenerator(_options, _database);

            generator.Next();
            var value = generator.Current;

            Assert.Equal((int)value, TestQueryResults.Values[0]);
            _database.CreateQueryExecutorFactory(Arg.Any<Table>(), Arg.Any<Query>()).Received(1);
        }

        private class TestQueryResults : IQueryResults
        {
            public static int[] Values = [1, 2, 3, 4, 5];

            private int _index = -1;

            public IEnumerable<string> ColumnNames => ["Id"];

            public object GetValue(string columnName) => Values[_index];

            public bool Read() => ++_index < Values.Length; 
        }
    }
}
