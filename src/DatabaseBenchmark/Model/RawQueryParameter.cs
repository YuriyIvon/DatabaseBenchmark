using DatabaseBenchmark.Common;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Model
{
    public class RawQueryParameter
    {
        public string Name { get; set; }

        public ColumnType Type { get; set; }

        public bool Array { get; set; }

        [JsonConverter(typeof(JsonObjectConverter))]
        public object Value { get; set; }

        public bool Collection { get; set; }

        public bool RandomizeValue { get; set; } = false;

        public ValueRandomizationRule ValueRandomizationRule { get; set; } = new ValueRandomizationRule();

        public bool Inline { get; set; } = false;

        public string InlineFormat { get; set; }
    }
}
