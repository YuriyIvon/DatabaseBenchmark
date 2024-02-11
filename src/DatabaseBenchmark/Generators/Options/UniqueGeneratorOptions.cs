using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Generators.Options
{
    public class UniqueGeneratorOptions : GeneratorOptionsBase
    {
        public override GeneratorType Type => GeneratorType.Unique;

        public int AttemptCount { get; set; } = 100;

        public IGeneratorOptions SourceGeneratorOptions { get; set; }

        public int MaxValues { get; set; } = 1000000;
    }
}
