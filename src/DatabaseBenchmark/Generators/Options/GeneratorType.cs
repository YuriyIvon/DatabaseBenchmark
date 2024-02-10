using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GeneratorType
    {
        Address,
        Boolean,
        Collection, //Not exposed through the factory yet
        Company,
        DataSourceIterator,
        DateTime,
        Finance,
        Float,
        ColumnItem,
        ColumnIterator,
        Guid,
        Integer,
        Internet,
        ListItem,
        ListIterator,
        Name,
        Null,
        Phone,
        String,
        Text,
        Unique,
        Vehicle
    }
}
