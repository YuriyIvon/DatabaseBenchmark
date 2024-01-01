using Bogus;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Core
{
    public class RandomValueProvider : IRandomValueProvider
    {
        //TODO: what to do with the faker
        private readonly Faker _faker = new();
        private readonly Dictionary<(ValueRandomizationRule, bool), IGenerator> _generators = [];

        private readonly IGeneratorFactory _generatorFactory;
        private readonly IColumnPropertiesProvider _columnPropertiesProvider;
        private readonly IDistinctValuesProvider _distinctValuesProvider;

        public RandomValueProvider(
            IGeneratorFactory randomGeneratorFactory,
            IColumnPropertiesProvider columnPropertiesProvider,
            IDistinctValuesProvider distinctValuesProvider)
        {
            _generatorFactory = randomGeneratorFactory;
            _columnPropertiesProvider = columnPropertiesProvider;
            _distinctValuesProvider = distinctValuesProvider;
        }

        public object GetRandomValue(string tableName, string columnName, ValueRandomizationRule randomizationRule) =>
            GetRandomValue(tableName, columnName, randomizationRule, false);

        public IEnumerable<object> GetRandomValueCollection(string tableName, string columnName, ValueRandomizationRule randomizationRule) =>
            (IEnumerable<object>)GetRandomValue(tableName, columnName, randomizationRule, true);

        private object GetRandomValue(string tableName, string columnName, ValueRandomizationRule randomizationRule, bool collection)
        {
            if (!_generators.TryGetValue((randomizationRule, collection), out var generator))
            {
                var options = randomizationRule.GeneratorOptions;

                if (randomizationRule.UseExistingValues)
                {
                    if (options != null && options is not ListItemGeneratorOptions)
                    {
                        throw new InputArgumentException("Only ListItem generator type is allowed when UseExistingValues is set");
                    }

                    var listItemGeneratorOptions = options as ListItemGeneratorOptions ?? new ListItemGeneratorOptions();
                    listItemGeneratorOptions.Items = _distinctValuesProvider.GetDistinctValues(tableName, columnName);

                    generator = _generatorFactory.Create(listItemGeneratorOptions);
                }
                else
                {
                    var columnType = _columnPropertiesProvider.GetColumnType(tableName, columnName);
                    options ??= GetDefaultGeneratorOptions(columnType);
                    generator = _generatorFactory.Create(options);
                }

                if (collection)
                {
                    generator = new CollectionGenerator(
                        _faker,
                        generator, 
                        new CollectionGeneratorOptions
                        {
                            MinLength = randomizationRule.MinCollectionLength,
                            MaxLength = randomizationRule.MaxCollectionLength
                        });
                }

                _generators.Add((randomizationRule, collection), generator);
            }

            return generator.Generate();
        }

        private static IGeneratorOptions GetDefaultGeneratorOptions(ColumnType columnType) =>
            columnType switch
            {
                ColumnType.Boolean => new BooleanGeneratorOptions(),
                ColumnType.Guid => new GuidGeneratorOptions(),
                ColumnType.Integer => new IntegerGeneratorOptions(),
                ColumnType.Double => new FloatGeneratorOptions(),
                ColumnType.Long => new IntegerGeneratorOptions(),
                ColumnType.DateTime => new DateTimeGeneratorOptions(),
                ColumnType.String => new StringGeneratorOptions(),
                ColumnType.Text => new StringGeneratorOptions(),
                _ => throw new InputArgumentException($"Unsupported random value type \"{columnType}\"")
            };
    }
}
