using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Generators.Options
{
    public class NullGeneratorOptions : GeneratorOptionsBase
    {
        public override GeneratorType Type => GeneratorType.Null;

        public float NullProbability { get; set; }

        public IGeneratorOptions SourceGeneratorOptions { get; set; }
    }
}
