using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Generators.Options
{
    public abstract class GeneratorOptionsBase : IGeneratorOptions
    {
        public abstract GeneratorType Type { get; }

        public float NullProbability { get; set; }
    }
}
