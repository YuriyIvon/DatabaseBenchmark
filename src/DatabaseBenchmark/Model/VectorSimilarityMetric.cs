using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Model
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum VectorSimilarityMetric
    {
        Cosine,
        Euclidean,
        DotProduct
    }
}
