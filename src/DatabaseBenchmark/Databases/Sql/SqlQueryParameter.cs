using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.Sql
{
    public record SqlQueryParameter
    {
        public char Prefix { get; }

        public string Name { get; }

        public object Value { get; }

        public ColumnType Type { get; }

        public SqlQueryParameter(char prefix, string name, object value, ColumnType type)
        {
            Prefix = prefix;
            Name = name;
            Value = value;
            Type = type;
        }
    }
}
