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
        Pattern,
        Phone,
        String,
        Text,
        Unique,
        Vehicle
    }
}
