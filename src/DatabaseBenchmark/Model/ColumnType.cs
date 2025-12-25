using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Model
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ColumnType
    {
        Boolean,
        Guid,
        Integer,
        Long,
        Double,
        String,
        Text,
        DateTime,
        Json,
        Vector
    }
}
