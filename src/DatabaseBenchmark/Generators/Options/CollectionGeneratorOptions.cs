using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Generators.Options
{
    public class CollectionGeneratorOptions : GeneratorOptionsBase
    {
        public override GeneratorType Type => GeneratorType.Collection;

        public int MinLength { get; set; }

        public int MaxLength { get; set; }

        public IGeneratorOptions SourceGeneratorOptions { get; set; }
    }
}
