using DatabaseBenchmark.Common;
using DatabaseBenchmark.Reporting;
using System;
using System.Collections.Generic;
using Xunit;

namespace DatabaseBenchmark.Tests.Reporting
{
    public class ResultsBuilderTests
    {
        private readonly string _name = "Sample Name";
        private readonly MetricsCollection _metrics;

        public ResultsBuilderTests()
        {
            _metrics = new MetricsCollection(_name);
        }

        [Fact]
        public void BuildResultsAllColumns()
        {
            InitializeWithoutCustomMetrics();

            var displayColumns = new string[]
            {
                "name",
                "avg",
                "stdDev",
                "min",
                "p10",
                "p50",
                "p90",
                "max",
                "qps",
                "avgRows"
            };
            var resultsBuilder = new ResultsBuilder(displayColumns);

            var results = resultsBuilder.Build(new[] { _metrics });

            Assert.Single(results.Rows);
            Assert.Equal(10, results.Columns.Count);
            Assert.Equal(_name, results.Rows[0]["name"]);
            Assert.Equal(2000.0, results.Rows[0]["avg"]);
            Assert.Equal(816.0, Math.Round((double)results.Rows[0]["stdDev"]));
            Assert.Equal(1000.0, results.Rows[0]["min"]);
            Assert.Equal(3000.0, results.Rows[0]["max"]);
            Assert.Equal(1000.0, results.Rows[0]["p10"]);
            Assert.Equal(2000.0, results.Rows[0]["p50"]);
            Assert.Equal(2000.0, results.Rows[0]["p90"]);
            Assert.Equal(1.0, results.Rows[0]["qps"]);
            Assert.Equal(2.0, results.Rows[0]["avgRows"]);
        }

        [Fact]
        public void BuildResultsDefaultColumns()
        {
            InitializeWithoutCustomMetrics();

            var resultsBuilder = new ResultsBuilder();

            var results = resultsBuilder.Build(new[] { _metrics });

            Assert.Single(results.Rows);
            Assert.Equal(8, results.Columns.Count);
            Assert.True(results.Columns.Contains("name"));
            Assert.True(results.Columns.Contains("avg"));
            Assert.True(results.Columns.Contains("stdDev"));
            Assert.True(results.Columns.Contains("min"));
            Assert.True(results.Columns.Contains("p50"));
            Assert.True(results.Columns.Contains("max"));
            Assert.True(results.Columns.Contains("qps"));
            Assert.True(results.Columns.Contains("avgRows"));
        }

        [Fact]
        public void BuildResultsNonExistingColumn()
        {
            InitializeWithoutCustomMetrics();

            Assert.Throws<InputArgumentException>(() => new ResultsBuilder(new string[] { "abc" }));
        }

        [Fact]
        public void BuildResultsCustomMetricsDefaultColumns()
        {
            InitializeWithCustomMetrics();

            var resultsBuilder = new ResultsBuilder(Array.Empty<string>());

            var results = resultsBuilder.Build(new[] { _metrics });

            Assert.Single(results.Rows);
            Assert.Equal(4, results.Columns.Count);
            Assert.Equal(200.0, results.Rows[0]["avgCM"]);
            Assert.Equal(82.0, Math.Round((double)results.Rows[0]["stdDevCM"]));
            Assert.Equal(100.0, results.Rows[0]["minCM"]);
            Assert.Equal(300.0, results.Rows[0]["maxCM"]);
        }

        [Fact]
        public void BuildResultsCustomMetricsSelectedColumns()
        {
            InitializeWithCustomMetrics();

            var displayCustomMetricsColumns = new string[]
            {
                "avg",
            };

            var resultsBuilder = new ResultsBuilder(Array.Empty<string>(), displayCustomMetricsColumns);

            var results = resultsBuilder.Build(new[] { _metrics });

            Assert.Single(results.Rows);
            Assert.Single(results.Columns);
            Assert.Equal(200.0, results.Rows[0]["avgCM"]);
        }

        [Fact]
        public void BuildResultsCustomMetricsNonExistingColumn()
        {
            InitializeWithCustomMetrics();

            Assert.Throws<InputArgumentException>(() => new ResultsBuilder(Array.Empty<string>(), new string[] { "abc" }));
        }

        private void InitializeWithoutCustomMetrics()
        {
            var currentTime = DateTime.Now;

            _metrics.Metrics.Add(new Metric(currentTime.AddSeconds(-1), currentTime, 1));
            _metrics.Metrics.Add(new Metric(currentTime.AddSeconds(-2), currentTime, 2));
            _metrics.Metrics.Add(new Metric(currentTime.AddSeconds(-3), currentTime, 3));
        }

        private void InitializeWithCustomMetrics()
        {
            var currentTime = DateTime.Now;
            var customMetric = "CM";

            _metrics.Metrics.Add(
                new Metric(
                    currentTime.AddSeconds(-1),
                    currentTime,
                    1,
                    new Dictionary<string, double> { [customMetric] = 100 }));
            _metrics.Metrics.Add(
                new Metric(
                    currentTime.AddSeconds(-2),
                    currentTime,
                    2,
                    new Dictionary<string, double> { [customMetric] = 200 }));
            _metrics.Metrics.Add(
                new Metric(
                    currentTime.AddSeconds(-3),
                    currentTime,
                    3,
                    new Dictionary<string, double> { [customMetric] = 300 }));
        }
    }
}
