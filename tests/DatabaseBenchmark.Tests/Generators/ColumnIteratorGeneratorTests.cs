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
    public class ColumnIteratorGeneratorTests
    {
        private readonly IDatabase _database;
        private readonly ColumnIteratorGeneratorOptions _options = new()
        {
            TableName = "Test",
            ColumnName = "Id",
            ColumnType = ColumnType.Integer
        };

        public ColumnIteratorGeneratorTests()
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
            var generator = new ColumnIteratorGenerator(_options, _database);
            var items = new List<object>();

            for (int i = 0; i < TestQueryResults.Values.Length + 10 && generator.Next(); i++)
            {
                items.Add(generator.Current);
            }

            Assert.Equal(TestQueryResults.Values, items.Select(i => (int)i));
            Assert.False(generator.Next());
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
