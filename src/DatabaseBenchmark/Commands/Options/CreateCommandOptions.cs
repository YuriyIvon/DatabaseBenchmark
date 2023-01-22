﻿using DatabaseBenchmark.Commands.Options.Interfaces;
using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Commands.Options
{
    public class CreateCommandOptions :
        IStructuredTargetOptions,
        IQueryTraceOptions
    {
        public string DatabaseType { get; set; }

        public string ConnectionString { get; set; }

        public string TableFilePath { get; set; }

        public string TableName { get; set; }

        public bool TraceQueries { get; set; } = false;

        [Option("Drop table if already exists")]
        public bool DropExisting { get; set; } = false;
    }
}
