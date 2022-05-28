using System.Data;

namespace DatabaseBenchmark.Core.Interfaces
{
    public interface IExecutionEnvironment
    {
        public bool TraceQueries { get; }

        public bool TraceResults { get; }

        public void Write(string text);

        public void WriteLine(string text);

        public void WriteTable(DataTable table);
    }
}
