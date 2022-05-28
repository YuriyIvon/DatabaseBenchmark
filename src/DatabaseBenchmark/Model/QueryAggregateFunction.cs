using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Model
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum QueryAggregateFunction
    {
        None,
        Average,
        Count,
        DistinctCount,
        Max,
        Min,
        Sum
    }
}
