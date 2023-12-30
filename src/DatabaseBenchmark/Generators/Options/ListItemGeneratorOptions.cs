using DatabaseBenchmark.Common;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    public class ListItemGeneratorOptions : GeneratorOptionsBase
    {
        public override GeneratorType Type => GeneratorType.ListItem;

        [JsonConverter(typeof(JsonObjectArrayConverter))]
        public object[] Items { get; set; }

        public WeightedListItem[] WeightedItems { get; set; }
    }
}
