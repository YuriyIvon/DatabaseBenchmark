using DatabaseBenchmark.Common;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    public class ConstantGeneratorOptions : GeneratorOptionsBase
    {
        public override GeneratorType Type => GeneratorType.Constant;

        [JsonConverter(typeof(JsonObjectConverter))]
        public object Value { get; set; }
    }
}
