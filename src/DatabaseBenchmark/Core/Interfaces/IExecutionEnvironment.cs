using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Core.Interfaces
{
    public interface IExecutionEnvironment
    {
        bool TraceQueries { get; }

        bool TraceResults { get; }

        IValueFormatter ValueFormatter {  get; }

        void Write(string text);

        void WriteLine(string text);

        void WriteTable(LightweightDataTable table);
    }
}
