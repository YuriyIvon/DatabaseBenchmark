using Bogus;
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
    public class ForeignColumnGeneratorTests
    {
        private readonly Faker _faker = new();
        private readonly IDatabase _database;
        private readonly ForeignColumnGeneratorOptions _options = new()
        {
            TableName = "Test",
            ColumnName = "Id",
            ColumnType = ColumnType.Integer
        };

        public ForeignColumnGeneratorTests()
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
        public void GenerateValueFromItems()
        {
            var generator = new ForeignColumnGenerator(_faker, _options, _database);

            var value = generator.Generate();

            Assert.Contains((int)value, TestQueryResults.Values);
            _database.CreateQueryExecutorFactory(Arg.Any<Table>(), Arg.Any<Query>()).Received(1);
        }

        [Fact]
        public void GenerateCollectionFromItems()
        {
            var generator = new ForeignColumnGenerator(_faker, _options, _database);

            var collection = generator.GenerateCollection(3);

            Assert.Equal(3, collection.Count());
            Assert.True(collection.All(i => TestQueryResults.Values.Contains((int)i)));
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
