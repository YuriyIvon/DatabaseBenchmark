using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Model
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum QueryPrimitiveOperator
    {
        Equals,
        NotEquals,
        Greater,
        GreaterEquals,
        Lower,
        LowerEquals,
        Contains,
        StartsWith,
        In
    }
}
