﻿namespace DatabaseBenchmark.Generators.Options
{
    public class DateTimeGeneratorOptions : GeneratorOptionsBase
    {
        public override GeneratorType Type => GeneratorType.DateTime;

        public DateTime MinValue { get; set; } = DateTime.Now;

        public DateTime MaxValue { get; set; } = DateTime.Now.AddYears(1);

        public bool Increasing { get; set; } = false;

        public TimeSpan Delta { get; set; } = TimeSpan.Zero;

        public bool RandomizeDelta { get; set; } = false;
    }
}
