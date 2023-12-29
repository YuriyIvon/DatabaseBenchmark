using DatabaseBenchmark.Model;
using System.Text.Json;

namespace DatabaseBenchmark.DataSources.Generator
{
    public class GeneratorDataSourceColumn
    {
        public string Name { get; set; }

        public JsonElement GeneratorOptions { get; set; }
    }
}
