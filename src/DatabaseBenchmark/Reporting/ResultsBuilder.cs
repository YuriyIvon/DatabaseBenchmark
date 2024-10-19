using DatabaseBenchmark.Common;
using DatabaseBenchmark.Reporting.Interfaces;
using DatabaseBenchmark.Reporting.Model;

namespace DatabaseBenchmark.Reporting
{
    public class ResultsBuilder : IResultsBuilder
    {
        private const string NameColumn = "name";
        private const string AvgColumn = "avg";
        private const string StdDevColumn = "stdDev";
        private const string MinColumn = "min";
        private const string P10Column = "p10";
        private const string P50Column = "p50";
        private const string P90Column = "p90";
        private const string MaxColumn = "max";
        private const string QpsColumn = "qps";
        private const string AvgRowsColumn = "avgRows";

        private readonly ResultColumn[] _columns = new[]
        {
            new ResultColumn
            { 
                Name = NameColumn,
                Caption = "Name",
                Type = typeof(string),
                ValueFunc = mc => mc.Name
            },
            new ResultColumn
            { 
                Name = AvgColumn,
                Caption = "Avg (ms)",
                Type = typeof(double),
                ValueFunc = mc => GetDurations(mc).Average()
            },
            new ResultColumn
            {
                Name = StdDevColumn,
                Caption = "StDev (ms)",
                Type = typeof(double),
                ValueFunc = mc => StdDev(GetDurations(mc))
            },
            new ResultColumn
            {
                Name = MinColumn,
                Caption = "Min (ms)",
                Type = typeof(double),
                ValueFunc = mc => GetDurations(mc).Min()
            },
            new ResultColumn
            {
                Name = P10Column,
                Caption = "10% (ms)",
                Type = typeof(double),
                ValueFunc = mc => GetDurations(mc).Percentile(10)
            },
            new ResultColumn
            {
                Name = P50Column,
                Caption = "50% (ms)",
                Type = typeof(double),
                ValueFunc = mc => GetDurations(mc).Percentile(50)
            },
            new ResultColumn
            {
                Name = P90Column,
                Caption = "90% (ms)",
                Type = typeof(double),
                ValueFunc = mc => GetDurations(mc).Percentile(90)
            },
            new ResultColumn
            {
                Name = MaxColumn,
                Caption = "Max (ms)",
                Type = typeof(double),
                ValueFunc = mc => GetDurations(mc).Max()
            },
            new ResultColumn
            {
                Name = QpsColumn,
                Caption = "QPS",
                Type = typeof(double),
                ValueFunc = mc => GetThroughput(mc)
            },
            new ResultColumn
            {
                Name = AvgRowsColumn,
                Caption = "Avg Rows",
                Type = typeof(double),
                ValueFunc = mc => mc.Metrics.Select(m => m.RowCount).Average()
            }
        };

        private readonly CustomMetricResultColumn[] _customMetricColumns = new[]
        {
            new CustomMetricResultColumn
            {
                Name = AvgColumn,
                Caption = "Avg",
                ValueFunc = (cm, mt) => cm.Average(m => m[mt])
            },
            new CustomMetricResultColumn
            {
                Name = StdDevColumn,
                Caption = "StDev",
                ValueFunc = (cm, mt) => StdDev(cm.Select(m => m[mt]))
            },
            new CustomMetricResultColumn
            {
                Name = MinColumn,
                Caption = "Min",
                ValueFunc = (cm, mt) => cm.Min(m => m[mt])
            },
            new CustomMetricResultColumn
            {
                Name = MaxColumn,
                Caption = "Max",
                ValueFunc = (cm, mt) => cm.Max(m => m[mt])
            }
        };

        private readonly string[] _defaultDisplayColumns = new[]
        {
            NameColumn,
            AvgColumn,
            StdDevColumn,
            MinColumn,
            P50Column,
            MaxColumn,
            QpsColumn,
            AvgRowsColumn
        };

        private readonly string[] _defaultDisplayCustomMetricColumns = new[]
        {
            AvgColumn,
            StdDevColumn,
            MinColumn,
            MaxColumn
        };

        private readonly string[] _displayColumns;
        private readonly string[] _displayCustomMetricColumns;

        public ResultsBuilder(
            string[] displayColumns = null,
            string[] displayCustomMetricColumns = null)
        {
            _displayColumns = displayColumns ?? _defaultDisplayColumns;
            _displayCustomMetricColumns = displayCustomMetricColumns ?? _defaultDisplayCustomMetricColumns;

            ValidateColumns();
            ValidateCustomMetricColumns();
        }
            
        public LightweightDataTable Build(IEnumerable<MetricsCollection> metrics)
        {
            var table = new LightweightDataTable();

            foreach (var metricsCollection in metrics)
            {
                var row = table.AddRow();

                foreach (var columnName in _displayColumns)
                {
                    AppendMetricColumn(row, columnName, metricsCollection);
                }

                var customMetrics = metricsCollection.Metrics
                    .Where(m => m.CustomMetrics != null)
                    .Select(m => m.CustomMetrics);

                var customMetricTypes = customMetrics.SelectMany(cm => cm.Keys).Distinct();

                foreach (var metricType in customMetricTypes)
                {
                    foreach (var columnName in _displayCustomMetricColumns)
                    {
                        AppendCustomMetricColumn(row, metricType, columnName, customMetrics);
                    }
                }
            }

            return table;
        }

        private void ValidateColumns()
        {
            foreach (var name in _displayColumns)
            {
                if (!_columns.Any(c => c.Name == name))
                {
                    throw new InputArgumentException($"Column \"{name}\" does not exist");
                }
            }
        }

        private void ValidateCustomMetricColumns()
        {
            foreach (var name in _displayCustomMetricColumns)
            {
                if (!_customMetricColumns.Any(c => c.Name == name))
                {
                    throw new InputArgumentException($"Custom metrics column \"{name}\" does not exist");
                }
            }
        }

        private void AppendMetricColumn(LightweightDataRow row, string name, MetricsCollection metricCollection)
        {
            var columnDefinition = _columns.FirstOrDefault(c => c.Name == name);

            if (!row.Table.HasColumn(name))
            {
                var column = row.Table.AddColumn(name);
                column.Caption = columnDefinition.Caption;
            }

            row[name] = columnDefinition.ValueFunc(metricCollection);
        }

        private void AppendCustomMetricColumn(
            LightweightDataRow row,
            string metricType,
            string prefix,
            IEnumerable<IDictionary<string, double>> customMetrics)
        {
            var columnDefinition = _customMetricColumns.FirstOrDefault(c => c.Name == prefix);

            var columnName = prefix + metricType;
            if (!row.Table.HasColumn(columnName))
            {
                var customMetricColumn = row.Table.AddColumn(prefix + metricType);
                customMetricColumn.Caption = $"{columnDefinition.Caption} ({metricType})";
            }

            row[columnName] = columnDefinition.ValueFunc(customMetrics, metricType);
        }

        private static double GetThroughput(MetricsCollection metrics)
        {
            var minTimestamp = metrics.Metrics.Min(m => m.StartTimestamp);
            var maxTimestamp = metrics.Metrics.Max(m => m.EndTimestamp);
            return metrics.Metrics.Count / (maxTimestamp - minTimestamp).TotalSeconds;
        }

        private static double StdDev(IEnumerable<double> values)
        {
            var average = values.Average();
            var diffSquares = values.Select(v => (v - average) * (v - average)).Sum();
            return Math.Sqrt(diffSquares / values.Count());
        }

        private static IEnumerable<double> GetDurations(MetricsCollection metricsCollection) =>
            metricsCollection.Metrics.Select(m => (m.EndTimestamp - m.StartTimestamp).TotalMilliseconds);

        private class ResultColumn
        {
            public string Name { get; init; }

            public string Caption { get; init; }

            public Type Type { get; init; }

            public Func<MetricsCollection, object> ValueFunc { get; init; }
        }

        private class CustomMetricResultColumn
        {
            public string Name { get; init; }

            public string Caption { get; init; }

            public Func<IEnumerable<IDictionary<string, double>>, string, object> ValueFunc { get; init; }
        }
    }
}
