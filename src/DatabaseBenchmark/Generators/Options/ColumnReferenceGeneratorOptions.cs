namespace DatabaseBenchmark.Generators.Options
{
    public class ColumnReferenceGeneratorOptions : GeneratorOptionsBase
    {
        public override GeneratorType Type => GeneratorType.ColumnReference;

        public string ColumnName { get; set; }
    }
}
