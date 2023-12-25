using DatabaseBenchmark.Generators.Interfaces;

namespace DatabaseBenchmark.Generators.Options
{
    public class ListItemGeneratorOptions : IGeneratorOptions
    {
        public GeneratorType Type => GeneratorType.ListItem;

        public object[] Items { get; set; }

        public WeightedListItem[] WeightedItems { get; set; }
    }
}
