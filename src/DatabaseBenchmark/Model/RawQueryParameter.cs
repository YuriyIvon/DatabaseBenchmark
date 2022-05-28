using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Model
{
    public class RawQueryParameter
    {
        public string Name { get; set; }

        public ColumnType Type { get; set; }

        [JsonConverter(typeof(JsonObjectConverter))]
        public object Value { get; set; }

        public bool Collection { get; set; }

        public bool RandomizeValue { get; set; } = false;

        public ValueRandomizationRule ValueRandomizationRule { get; set; } = new ValueRandomizationRule();
    }
}
