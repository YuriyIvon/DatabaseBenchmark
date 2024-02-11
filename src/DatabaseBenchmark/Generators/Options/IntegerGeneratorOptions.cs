namespace DatabaseBenchmark.Generators.Options
{
    public class IntegerGeneratorOptions : GeneratorOptionsBase
    {
        public override GeneratorType Type => GeneratorType.Integer;

        public int MinValue { get; set; } = 0;

        public int MaxValue { get; set; } = 100;

        public Direction Direction { get; set; } = Direction.None;

        public int Delta { get; set; } = 0;

        public bool RandomizeDelta { get; set; } = false;
    }
}
