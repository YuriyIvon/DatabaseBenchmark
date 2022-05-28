using DatabaseBenchmark.Core.Interfaces;

namespace DatabaseBenchmark.Databases.Common
{
    public class ImportProgressReporter : IProgressReporter
    {
        const int _interval = 2000;

        private readonly IExecutionEnvironment _environment;

        private int _rowCount = 0;
        private DateTime? _lastTimestamp;

        public ImportProgressReporter(IExecutionEnvironment environment)
        {
            _environment = environment;
        }

        public void Increment(int increment)
        {
            _rowCount += increment;

            var currentTimestamp = DateTime.Now;

            if (_lastTimestamp == null)
            {
                _lastTimestamp = currentTimestamp;
            }
            else if ((currentTimestamp - _lastTimestamp.Value).TotalMilliseconds > _interval)
            {
                _environment.WriteLine($"Imported {_rowCount} rows");
                _lastTimestamp = currentTimestamp;
            }
        }
    }
}
