namespace DatabaseBenchmark.Common
{
    public class LightweightDataColumn
    {
        public string Name { get; set; }

        public string Caption { get; set; }

        public LightweightDataTable Table { get; }

        public LightweightDataColumn(string name, LightweightDataTable table)
        {
            Name = name;
            Caption = name;
            Table = table;
        }
    }
}
