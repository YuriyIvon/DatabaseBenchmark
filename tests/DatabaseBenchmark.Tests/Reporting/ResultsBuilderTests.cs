using DatabaseBenchmark.Reporting;
using System;
using Xunit;

namespace DatabaseBenchmark.Tests.Reporting
{
    public class ResultsBuilderTests
    {
        [Fact]
        public void BuildResults()
        {
            string name = "Sample Name";
            var metrics = new MetricsCollection(name);
            var currentTime = DateTime.Now;
            metrics.Metrics.Add(new Metric(currentTime.AddSeconds(-1), currentTime, 1));
            metrics.Metrics.Add(new Metric(currentTime.AddSeconds(-2), currentTime, 2));
            metrics.Metrics.Add(new Metric(currentTime.AddSeconds(-3), currentTime, 3));
            var resultsBuilder = new ResultsBuilder();

            var results = resultsBuilder.Build(new[] { metrics });

            Assert.Single(results.Rows);
            Assert.Equal(9, results.Columns.Count);
            Assert.Equal(name, results.Rows[0]["name"]);
            Assert.Equal(2000.0, results.Rows[0]["avg"]);
            Assert.Equal(1000.0, results.Rows[0]["min"]);
            Assert.Equal(3000.0, results.Rows[0]["max"]);
            Assert.Equal(1000.0, results.Rows[0]["p10"]);
            Assert.Equal(2000.0, results.Rows[0]["p50"]);
            Assert.Equal(2000.0, results.Rows[0]["p90"]);
            Assert.Equal(1.0, results.Rows[0]["qps"]);
            Assert.Equal(2.0, results.Rows[0]["avgRows"]);
        }
    }
}
