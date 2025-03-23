namespace DatabaseBenchmark.Common
{
    public class LightweightDataRow
    {
        private Dictionary<string, object> _columnValues = [];

        public object this[string columnName]
        {
            get => _columnValues.GetValueOrDefault(columnName);
            set => _columnValues[columnName] = value;
        }

        public LightweightDataTable Table { get; }

        public LightweightDataRow(LightweightDataTable table)
        {
            Table = table;
        }
    }
}
