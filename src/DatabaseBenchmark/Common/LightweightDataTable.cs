namespace DatabaseBenchmark.Common
{
    public class LightweightDataTable
    {
        private List<LightweightDataColumn> _columns = [];
        private List<LightweightDataRow> _rows = [];

        public List<LightweightDataColumn> Columns => _columns;

        public List<LightweightDataRow> Rows => _rows;

        public LightweightDataRow AddRow()
        {
            var row = new LightweightDataRow(this);
            _rows.Add(row);

            return row;
        }

        public LightweightDataColumn AddColumn(string name)
        {
            var column = new LightweightDataColumn(name, this);
            _columns.Add(column);

            return column;
        }

        public bool HasColumn(string columnName) => Columns.Any(c => c.Name == columnName);
    }
}
