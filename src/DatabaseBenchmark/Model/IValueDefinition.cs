namespace DatabaseBenchmark.Model
{
    public interface IValueDefinition
    {
        public string Name { get; set; }
        
        public ColumnType Type { get; set; }
        
        public bool Array { get; set; }
    }
}
