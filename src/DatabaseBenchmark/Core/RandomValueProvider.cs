using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Model;
using System.Collections.Concurrent;

namespace DatabaseBenchmark.Core
{
    public class RandomValueProvider : IRandomValueProvider
    {
        private readonly IRandomGenerator _randomGenerator;
        private readonly IColumnPropertiesProvider _columnPropertiesProvider;
        private readonly IDistinctValuesProvider _distinctValuesProvider;
        private readonly ConcurrentDictionary<(string, string), object[]> _existingValues = new();

        public RandomValueProvider(
            IRandomGenerator randomGenerator,
            IColumnPropertiesProvider columnPropertiesProvider,
            IDistinctValuesProvider distinctValuesProvider)
        {
            _randomGenerator = randomGenerator;
            _columnPropertiesProvider = columnPropertiesProvider;
            _distinctValuesProvider = distinctValuesProvider;
        }

        public object GetRandomValue(string tableName, string columnName, ValueRandomizationRule randomizationRule)
        {
            if (randomizationRule.UseExistingValues)
            {
                var values = GetExistingValues(tableName, columnName, randomizationRule);
                return values[_randomGenerator.GetRandomInteger(0, values.Length - 1)];
            }
            else
            {
                return GetRandomPrimitiveValue(tableName, columnName, randomizationRule);
            }
        }

        public IEnumerable<object> GetRandomValueCollection(string tableName, string columnName, ValueRandomizationRule randomizationRule)
        {
            var length = _randomGenerator.GetRandomInteger(randomizationRule.MinCollectionLength, randomizationRule.MaxCollectionLength);

            if (randomizationRule.UseExistingValues)
            {
                var values = (object[])GetExistingValues(tableName, columnName, randomizationRule).Clone();
                Shuffle(values);
                return values.Take(length);
            }
            else
            {
                return Enumerable.Range(1, length)
                    .Select(_ => GetRandomPrimitiveValue(tableName, columnName, randomizationRule))
                    .ToArray();
            }
        }

        private object GetRandomPrimitiveValue(string tableName, string columnName, ValueRandomizationRule randomizationRule)
        {
            var columnType = _columnPropertiesProvider.GetColumnType(tableName, columnName);

            return columnType switch
            {
                ColumnType.Boolean => _randomGenerator.GetRandomBoolean(),
                ColumnType.Integer => _randomGenerator.GetRandomInteger(randomizationRule.MinNumericValue, randomizationRule.MaxNumericValue),
                ColumnType.Double => _randomGenerator.GetRandomDouble(randomizationRule.MinNumericValue, randomizationRule.MaxNumericValue),
                ColumnType.String => _randomGenerator.GetRandomString(
                    randomizationRule.MinStringValueLength,
                    randomizationRule.MaxStringValueLength,
                    randomizationRule.AllowedCharacters),
                ColumnType.Text => _randomGenerator.GetRandomString(
                    randomizationRule.MinStringValueLength,
                    randomizationRule.MaxStringValueLength,
                    randomizationRule.AllowedCharacters),
                ColumnType.DateTime => _randomGenerator.GetRandomDateTime(
                    randomizationRule.MinDateTimeValue,
                    randomizationRule.MaxDateTimeValue,
                    randomizationRule.DateTimeValueStep),
                _ => throw new InputArgumentException($"Unsupported random value type \"{columnType}\"")
            };
        }

        private object[] GetExistingValues(string tableName, string columnName, ValueRandomizationRule randomizationRule)
        {
            if (randomizationRule.ExistingValuesOverride == null)
            {
                var sourceTableName = randomizationRule.ExistingValuesSourceTableName ?? tableName;
                var sourceColumnName = randomizationRule.ExistingValuesSourceColumnName ?? columnName;

                if (sourceTableName == null)
                {
                    throw new InputArgumentException($"Table name is not specified for the column \"{sourceColumnName}\"");
                }

                if (!_existingValues.TryGetValue((sourceTableName, sourceColumnName), out var values))
                {
                    values = _distinctValuesProvider.GetDistinctValues(sourceTableName, sourceColumnName).ToArray();
                    _existingValues.TryAdd((sourceTableName, sourceColumnName), values);
                }

                return values;
            }
            else
            {
                return randomizationRule.ExistingValuesOverride;
            }
        }

        private void Shuffle<T>(T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                n--;
                int k = _randomGenerator.GetRandomInteger(0, n);
                T value = array[k];
                array[k] = array[n];
                array[n] = value;
            }
        }
    }
}
