using DatabaseBenchmark.Common;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    public class WeightedListItem
    {
        [JsonConverter(typeof(JsonObjectConverter))]
        public object Value { get; set; }

        public float Weight { get; set; }
    }
}
