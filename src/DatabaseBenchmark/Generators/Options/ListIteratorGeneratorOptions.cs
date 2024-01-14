using DatabaseBenchmark.Common;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    public class ListIteratorGeneratorOptions : GeneratorOptionsBase
    {
        public override GeneratorType Type => GeneratorType.ListIterator;

        [JsonConverter(typeof(JsonObjectArrayConverter))]
        public object[] Items { get; set; }
    }
}
