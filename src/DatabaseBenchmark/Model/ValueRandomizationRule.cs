using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Model
{
    public class ValueRandomizationRule
    {
        public bool UseExistingValues { get; set; } = true;

        [JsonConverter(typeof(GeneratorOptionsConverter))]
        public IGeneratorOptions GeneratorOptions { get; set; }

        public int MinCollectionLength { get; set; } = 1;

        public int MaxCollectionLength { get; set; } = 10;
    }
}
