using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Generators.Options
{
    public class ForeignColumnGeneratorOptions : IGeneratorOptions
    {
        public GeneratorType Type => GeneratorType.ForeignColumn;

        public string TableName { get; set; }

        public string ColumnName { get; set; }

        public bool Distinct { get; set; } = false;

        //TODO: can we avoid using this field?
        public ColumnType ColumnType { get; set; }

        public WeightedListItem[] WeightedItems { get; set; }
    }
}
