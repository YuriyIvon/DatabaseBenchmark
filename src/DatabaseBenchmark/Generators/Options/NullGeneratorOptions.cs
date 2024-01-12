using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Generators.Options
{
    public class NullGeneratorOptions : GeneratorOptionsBase
    {
        public override GeneratorType Type => GeneratorType.Null;

        public float Weight { get; set; } = 0.5f;

        public IGeneratorOptions SourceGeneratorOptions { get; set; }
    }
}
