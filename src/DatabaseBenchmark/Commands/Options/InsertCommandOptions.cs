using DatabaseBenchmark.Commands.Options.Interfaces;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;

namespace DatabaseBenchmark.Commands.Options
{
    internal class InsertCommandOptions :
        IStructuredTargetOptions,
        IDataSourceOptions,
        IQueryExecutionOptions,
        IReportOptions,
        IQueryTraceOptions
    {
        public string DatabaseType { get; set; }

        public string ConnectionString { get; set; }

        public string TableFilePath { get; set; }

        public string TableName { get; set; }

        public string DataSourceType { get; set; }

        public string DataSourceFilePath { get; set; }

        public int DataSourceMaxRows { get; set; } = 0;

        public string MappingFilePath { get; set; }

        public string BenchmarkName { get; set; }

        public int QueryParallelism { get; set; } = 1;

        public int QueryCount { get; set; } = 100;

        public int WarmupQueryCount { get; set; } = 3;

        public int QueryDelay { get; set; } = 0;

        public string ReportFormatterType { get; set; } = "Text";

        public string ReportFilePath { get; set; }

        public string[] ReportColumns { get; set; }

        public string[] ReportCustomMetricColumns { get; set; }

        public bool TraceQueries { get; set; } = false;

        [Option("Number of records inserted into the database in one query")]
        public int BatchSize { get; set; } = 1;
    }
}
