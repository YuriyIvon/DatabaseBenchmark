using DatabaseBenchmark.Generators.Interfaces;

namespace DatabaseBenchmark.Generators.Options
{
    public class DateTimeGeneratorOptions : IGeneratorOptions
    {
        public GeneratorType Type => GeneratorType.DateTime;

        public DateTime MinValue { get; set; } = DateTime.Now;

        public DateTime MaxValue { get; set; } = DateTime.Now.AddYears(1);

        public bool Increasing { get; set; } = false;

        public TimeSpan Delta { get; set; } = TimeSpan.FromDays(1);

        public bool RandomizeDelta { get; set; } = false;
    }
}
