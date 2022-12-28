using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Reporting;
using NSubstitute;
using Xunit;

namespace DatabaseBenchmark.Tests.Common
{
    public class QueryBenchmarkTests
    {
        [Fact]
        public void QueryBenchmarkExecutionCount()
        {
            var environment = Substitute.For<IExecutionEnvironment>();
            var metricsCollector = new MetricsCollector();
            var queryBenchmark = new QueryBenchmark(environment, metricsCollector);

            var preparedQuery = Substitute.For<IPreparedQuery>();
            var results = Substitute.For<IQueryResults>();
            results.Read().Returns(false);
            preparedQuery.Results.Returns(results);
            var queryExecutor = Substitute.For<IQueryExecutor>();
            queryExecutor.Prepare().Returns(preparedQuery);
            var queryExecutorFactory = Substitute.For<IQueryExecutorFactory>();
            queryExecutorFactory.Create().Returns(queryExecutor);

            var options = new QueryExecutionOptions
            {
                QueryCount = 5,
                QueryParallelism = 2,
                WarmupQueryCount = 2
            };

            queryBenchmark.Benchmark(queryExecutorFactory, options);

            preparedQuery.Received(14).Execute();
            results.Received(14).Read();
            preparedQuery.Received(14).Dispose();
        }
    }
}
