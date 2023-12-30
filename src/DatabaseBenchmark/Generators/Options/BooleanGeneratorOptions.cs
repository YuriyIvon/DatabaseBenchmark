namespace DatabaseBenchmark.Generators.Options
{
    public class BooleanGeneratorOptions : GeneratorOptionsBase
    {
        public override GeneratorType Type => GeneratorType.Boolean;

        public float? Weight { get; set; }
    }
}
