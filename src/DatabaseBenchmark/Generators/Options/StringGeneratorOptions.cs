namespace DatabaseBenchmark.Generators.Options
{
    public class StringGeneratorOptions : GeneratorOptionsBase
    {
        public override GeneratorType Type => GeneratorType.String;

        public int MinLength { get; set; } = 1;

        public int MaxLength { get; set; } = 10;

        public string AllowedCharacters { get; set; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
    }
}
