namespace DatabaseBenchmark.Model
{
    public class Column : IValueDefinition
    {
        public string Name { get; set; }

        public ColumnType Type { get; set; }

        public bool Array { get; set; } = false;

        public bool Nullable { get; set; } = true;

        public bool Queryable { get; set; } = true;

        public bool PrimaryKey { get; set; } = false;

        public bool PartitionKey { get; set; } = false;

        public bool SortKey { get; set; } = false;

        public bool DatabaseGenerated { get; set; } = false;
    }
}
