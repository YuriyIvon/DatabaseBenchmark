using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.Sql.Interfaces
{
    public record SqlQueryParameter
    {
        public char Prefix { get; }

        public string Name { get; }

        public object Value { get; }

        public ColumnType Type { get; }

        public bool Array { get; }

        public SqlQueryParameter(char prefix, string name, object value, ColumnType type, bool array = false)
        {
            Prefix = prefix;
            Name = name;
            Value = value;
            Type = type;
            Array = array;
        }
    }
}
