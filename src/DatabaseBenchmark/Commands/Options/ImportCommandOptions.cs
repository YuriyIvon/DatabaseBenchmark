using DatabaseBenchmark.Commands.Options.Interfaces;
using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Commands.Options
{
    public class ImportCommandOptions :
        IStructuredTargetOptions,
        IDataSourceOptions,
        IQueryTraceOptions,
        IPluginOptions
    {
        public string DatabaseType { get; set; }

        public string ConnectionString { get; set; }

        public string TableFilePath { get; set; }

        public string TableName { get; set; }

        public string DataSourceType { get; set; }

        public string DataSourceFilePath { get; set; }

        public string DataSourceCulture { get; set; }

        public int DataSourceMaxRows { get; set; } = 0;

        public string MappingFilePath { get; set; }

        [Option("A database script to be executed after the source data has been imported")]
        public string PostScriptFilePath { get; set; }

        public bool TraceQueries { get; set; } = false;

        public string PluginsFilePath { get; set; }

        [Option("Number of records in a batch inserted into the database during import (each database type has its own default)")]
        public int BatchSize { get; set; } = 0;
    }
}
