namespace DatabaseBenchmark.Generators.Options
{
    public class FloatGeneratorOptions : GeneratorOptionsBase
    {
        public override GeneratorType Type => GeneratorType.Float;

        public double MinValue { get; set; } = 0;

        public double MaxValue { get; set; } = 100;

        public bool Increasing { get; set; } = false;

        public double Delta { get; set; } = 0;

        public bool RandomizeDelta { get; set; } = false;

        //TODO: implement logic
        public bool Unique { get; set; } = false;
    }
}
