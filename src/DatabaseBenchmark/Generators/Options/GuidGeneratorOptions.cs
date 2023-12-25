using DatabaseBenchmark.Generators.Interfaces;

namespace DatabaseBenchmark.Generators.Options
{
    public class GuidGeneratorOptions : IGeneratorOptions
    {
        public GeneratorType Type => GeneratorType.Guid;
    }
}
