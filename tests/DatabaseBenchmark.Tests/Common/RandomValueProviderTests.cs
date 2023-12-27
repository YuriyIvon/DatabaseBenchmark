using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Tests.Utils;
using NSubstitute;
using System;
using Xunit;

namespace DatabaseBenchmark.Tests.Common
{
    public class RandomValueProviderTests
    {
        private readonly string _tableName;
        private readonly string _distinctColumnName;
        private readonly object[] _distinctValues;
        private readonly IGeneratorFactory _randomGeneratorFactory;
        private readonly IRandomValueProvider _randomValueProvider;

        public RandomValueProviderTests()
        {
            var table = SampleInputs.Table;
            _tableName = table.Name;
            _distinctColumnName = "Category";
            _distinctValues = ["One", "Two", "Three", "Four", "Five", "Six", "Seven"];

            var columnPropertiesProvider = new TableColumnPropertiesProvider(table);
            _randomGeneratorFactory = new GeneratorFactory(null);
            var distinctValuesProvider = Substitute.For<IDistinctValuesProvider>();
            distinctValuesProvider.GetDistinctValues(_tableName, _distinctColumnName).Returns(_distinctValues);
            _randomValueProvider = new RandomValueProvider(_randomGeneratorFactory, columnPropertiesProvider, distinctValuesProvider);
        }

        [Fact]
        public void GenerateValueStringColumnDefaultRule()
        {
            var randomizationRule = new ValueRandomizationRule();
            var value = _randomValueProvider.GetRandomValue(_tableName, _distinctColumnName, randomizationRule);

            Assert.Contains(value, _distinctValues);
        }

        [Fact]
        public void GenerateValueStringColumn()
        {
            var randomizationRule = new ValueRandomizationRule
            {
                UseExistingValues = false,
                GeneratorOptions = new StringGeneratorOptions()
            };

            var value = _randomValueProvider.GetRandomValue(_tableName, _distinctColumnName, randomizationRule);

            Assert.IsType<string>(value);
        }

        [Fact]
        public void GenerateValueIntegerColumn()
        {
            const string integerColumn = "Count";

            var randomizationRule = new ValueRandomizationRule
            {
                UseExistingValues = false,
                GeneratorOptions = new IntegerGeneratorOptions()
            };

            var value = _randomValueProvider.GetRandomValue(_tableName, integerColumn, randomizationRule);

            Assert.IsType<int>(value);
        }

        [Fact]
        public void GenerateValueDoubleColumn()
        {
            const string doubleColumn = "Price";

            var randomizationRule = new ValueRandomizationRule
            {
                UseExistingValues = false,
                GeneratorOptions = new FloatGeneratorOptions()
            };

            var value = _randomValueProvider.GetRandomValue(_tableName, doubleColumn, randomizationRule);

            Assert.IsType<double>(value);
        }

        [Fact]
        public void GenerateValueDateTimeColumnDefaultRule()
        {
            const string dateTimeColumn = "CreatedAt";

            var randomizationRule = new ValueRandomizationRule
            {
                UseExistingValues = false,
                GeneratorOptions = new DateTimeGeneratorOptions()
            };

            var value = _randomValueProvider.GetRandomValue(_tableName, dateTimeColumn, randomizationRule);

            Assert.IsType<DateTime>(value);
        }
    }
}
