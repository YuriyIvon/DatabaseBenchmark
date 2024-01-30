namespace DatabaseBenchmark.Generators.Options
{
    public class DataSourceIteratorGeneratorOptions : GeneratorOptionsBase
    {
        public override GeneratorType Type => GeneratorType.DataSourceIterator;

        public string DataSourceType { get; set; }

        public string DataSourceFilePath { get; set; }

        public string ColumnName { get; set; }
    }
}
