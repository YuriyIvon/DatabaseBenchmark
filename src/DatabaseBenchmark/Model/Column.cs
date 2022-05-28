namespace DatabaseBenchmark.Model
{
    public class Column
    {
        public string Name { get; set; }

        public ColumnType Type { get; set; }

        public bool Nullable { get; set; } = true;

        public bool Queryable { get; set; } = true;

        public bool PartitionKey { get; set; } = false;

        public bool DatabaseGenerated { get; set; } = false;
    }
}
