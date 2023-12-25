using DatabaseBenchmark.Generators.Interfaces;

namespace DatabaseBenchmark.Generators.Options
{
    public class BooleanGeneratorOptions : IGeneratorOptions
    {
        public GeneratorType Type => GeneratorType.Boolean;

        public float Weight { get; set; }
    }
}
