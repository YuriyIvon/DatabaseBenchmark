using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Generators.Options
{
    public class ForeignKeyGeneratorOptions : IGeneratorOptions
    {
        public GeneratorType Type => GeneratorType.ForeignKey;

        public string TableName { get; set; }

        public string ColumnName { get; set; }

        //TODO: can we avoid using this field?
        public ColumnType ColumnType { get; set; }

        public WeightedListItem[] WeightedItems { get; set; }
    }
}
