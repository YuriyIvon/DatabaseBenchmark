using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Model
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum QuerySortDirection
    {
        Ascending,
        Descending
    }
}
