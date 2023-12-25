using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.DataSources.Generator
{
    public class GeneratorDataSourceColumn
    {
        public string Name { get; set; }

        [JsonConverter(typeof(GeneratorOptionsConverter))]
        public IGeneratorOptions GeneratorOptions { get; set; }
    }
}
