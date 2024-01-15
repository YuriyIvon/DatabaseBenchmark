using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Direction
    {
        None,
        Ascending,
        Descending
    }
}
