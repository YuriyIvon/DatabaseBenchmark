using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.DataSources.Generator
{
    public class GeneratorDataSourceColumn
    {
        public string Name { get; set; }

        public IGeneratorOptions GeneratorOptions { get; set; }
    }
}
