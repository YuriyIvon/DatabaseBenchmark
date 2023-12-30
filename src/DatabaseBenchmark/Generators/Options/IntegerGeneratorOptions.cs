using DatabaseBenchmark.Generators.Interfaces;

namespace DatabaseBenchmark.Generators.Options
{
    public class IntegerGeneratorOptions : IGeneratorOptions
    {
        public GeneratorType Type => GeneratorType.Integer;

        public int MinValue { get; set; } = 0;

        public int MaxValue { get; set; } = 100;

        public bool Increasing { get; set; } = false;

        public int Delta { get; set; } = 0;

        public bool RandomizeDelta { get; set; } = false;

        //TODO: implement logic
        public bool Unique { get; set; } = false;
    }
}
