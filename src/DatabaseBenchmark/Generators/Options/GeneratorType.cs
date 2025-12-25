using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GeneratorType
    {
        Address,
        Boolean,
        Collection,
        Company,
        Constant,
        DataSourceIterator,
        DateTime,
        Embedding,
        Finance,
        Float,
        ColumnItem,
        ColumnIterator,
        ColumnReference,
        Guid,
        Integer,
        Internet,
        ListItem,
        ListIterator,
        Name,
        Null,
        Pattern,
        Phone,
        String,
        Text,
        Unique,
        Vehicle
    }
}
