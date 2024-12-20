﻿using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Reporting;
using System.Data;

namespace DatabaseBenchmark.Core
{
    public class ExecutionEnvironment : IExecutionEnvironment
    {
        private readonly TextTableReportFormatter _tableFormatter = new();

        public bool TraceQueries { get; }

        public bool TraceResults { get; }

        public ExecutionEnvironment(bool traceQueries, bool traceResults = false)
        {
            TraceQueries = traceQueries;
            TraceResults = traceResults;
        }

        public void Write(string text) => Console.Write(text);

        public void WriteLine(string text) => Console.WriteLine(text);

        public void WriteTable(LightweightDataTable table)
        {
            var output = Console.OpenStandardOutput();
            _tableFormatter.Print(output, table);
        }
    }
}
