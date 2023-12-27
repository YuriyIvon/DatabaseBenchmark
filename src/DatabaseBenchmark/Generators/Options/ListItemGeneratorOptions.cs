using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    public class ListItemGeneratorOptions : IGeneratorOptions
    {
        public GeneratorType Type => GeneratorType.ListItem;

        [JsonConverter(typeof(JsonObjectArrayConverter))]
        public object[] Items { get; set; }

        public WeightedListItem[] WeightedItems { get; set; }
    }
}
