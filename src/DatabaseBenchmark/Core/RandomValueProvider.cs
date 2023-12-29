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
                var generatorOptions = GeneratorOptionsDeserializer.Deserialize(randomizationRule.GeneratorOptions);

                if (randomizationRule.UseExistingValues)
                {
                    if (generatorOptions != null && generatorOptions is not ListItemGeneratorOptions)
                    {
                        throw new InputArgumentException("Only ListItem generator type is allowed when UseExistingValues is set");
                    }

                    var options = generatorOptions as ListItemGeneratorOptions ?? new ListItemGeneratorOptions();
                    options.Items = _distinctValuesProvider.GetDistinctValues(tableName, columnName);

                    generator = new ListItemGenerator(_faker, options);
                }
                else
                {
                    var columnType = _columnPropertiesProvider.GetColumnType(tableName, columnName);
                    generatorOptions ??= GetDefaultGeneratorOptions(columnType);
                    generator = _generatorFactory.Create(generatorOptions.Type, generatorOptions);
                }

                if (collection)
                {
                    generator = new CollectionGeneratorDecorator(_faker, generator, randomizationRule.MaxCollectionLength, randomizationRule.MaxCollectionLength);
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
