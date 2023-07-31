using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.DynamoDb.Interfaces;

namespace DatabaseBenchmark.Databases.DynamoDb
{
    public class DynamoDbDataMetricsProvider :
        IDataMetricsProvider,
        IDynamoDbMetricsReporter
    {
        private long _rowCount = 0;

        public long GetRowCount() => _rowCount;

        public IDictionary<string, double> GetMetrics() => null;

        //Though interlocked operations add overhead to the insert query's total duration,
        //it's negligible compared to the insert's overall time.
        public void IncrementRowCount(long count) => Interlocked.Add(ref _rowCount, count);
    }
}
