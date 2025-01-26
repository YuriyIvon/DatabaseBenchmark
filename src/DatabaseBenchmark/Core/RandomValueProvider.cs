using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Core
{
    public class RandomValueProvider : IRandomValueProvider
    {
        private readonly Dictionary<(ValueRandomizationRule, bool), IGenerator> _generators = [];

        private readonly IGeneratorFactory _generatorFactory;
        private readonly IDistinctValuesProvider _distinctValuesProvider;

        public RandomValueProvider(
            IGeneratorFactory randomGeneratorFactory,
            IDistinctValuesProvider distinctValuesProvider)
        {
            _generatorFactory = randomGeneratorFactory;
            _distinctValuesProvider = distinctValuesProvider;
        }

        public void Next()
        {
            foreach (var generator in _generators.Values)
            {
                if (!generator.Next())
                {
                    throw new NoDataAvailableException();
                }
            }
        }

        public object GetValue(string tableName, IValueDefinition valueDefinition, ValueRandomizationRule randomizationRule) =>
            GetValue(tableName, valueDefinition, randomizationRule, false);

        public IEnumerable<object> GetValueCollection(string tableName, IValueDefinition valueDefinition, ValueRandomizationRule randomizationRule) =>
            (IEnumerable<object>)GetValue(tableName, valueDefinition, randomizationRule, true);

        private object GetValue(string tableName, IValueDefinition valueDefinition, ValueRandomizationRule randomizationRule, bool collection)
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
                    listItemGeneratorOptions.Items = _distinctValuesProvider.GetDistinctValues(
                        tableName,
                        valueDefinition,
                        valueDefinition.Array && randomizationRule.UnfoldArrayValues);

                    generator = _generatorFactory.Create(listItemGeneratorOptions);
                }
                else
                {
                    options ??= GetDefaultGeneratorOptions(valueDefinition.Type);
                    generator = _generatorFactory.Create(options);
                }

                if (collection)
                {
                    generator = new CollectionGenerator(
                        new CollectionGeneratorOptions
                        {
                            MinLength = randomizationRule.MinCollectionLength,
                            MaxLength = randomizationRule.MaxCollectionLength
                        },
                        generator);
                }

                _generators.Add((randomizationRule, collection), generator);

                if (!generator.Next())
                {
                    throw new NoDataAvailableException();
                }
            }

            return generator.Current;
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
