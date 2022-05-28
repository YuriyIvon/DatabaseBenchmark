using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Tests.Utils;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DatabaseBenchmark.Tests.Common
{
    public class RandomValueProviderTests
    {
        private readonly string _tableName;
        private readonly string _distinctColumnName;
        private readonly List<object> _distinctValues;
        private readonly IRandomGenerator _randomGenerator;
        private readonly IRandomValueProvider _randomValueProvider;

        public RandomValueProviderTests()
        {
            var table = SampleInputs.Table;
            _tableName = table.Name;
            _distinctColumnName = "Category";
            var columnPropertiesProvider = new TableColumnPropertiesProvider(table);
            _randomGenerator = Substitute.For<IRandomGenerator>();
            _distinctValues = new List<object> { "One", "Two", "Three", "Four", "Five", "Six", "Seven" };
            var distinctValuesProvider = Substitute.For<IDistinctValuesProvider>();
            distinctValuesProvider.GetDistinctValues(_tableName, _distinctColumnName).Returns(_distinctValues);
            _randomValueProvider = new RandomValueProvider(_randomGenerator, columnPropertiesProvider, distinctValuesProvider);
        }

        [Fact]
        public void GenerateValueStringColumnDefaultRule()
        {
            _randomGenerator.GetRandomInteger(Arg.Any<int>(), Arg.Any<int>()).Returns(1);
            var randomizationRule = new ValueRandomizationRule();
            var value = _randomValueProvider.GetRandomValue(_tableName, _distinctColumnName, randomizationRule);

            Assert.Equal(_distinctValues[1], value);
        }

        [Fact]
        public void GenerateValueStringColumnAlternativeExistingValuesSource()
        {
            var tableName = "AltTable";
            var columnName = "AltColumn";

            var columnPropertiesProvider = Substitute.For<IColumnPropertiesProvider>();
            var distinctValuesProvider = Substitute.For<IDistinctValuesProvider>();
            distinctValuesProvider.GetDistinctValues(Arg.Any<string>(), Arg.Any<string>()).Returns(_distinctValues);
            var randomValueProvider = new RandomValueProvider(_randomGenerator, columnPropertiesProvider, distinctValuesProvider);
            var randomizationRule = new ValueRandomizationRule
            {
                ExistingValuesSourceColumnName = columnName,
                ExistingValuesSourceTableName = tableName
            };

            _ = randomValueProvider.GetRandomValue(_tableName, _distinctColumnName, randomizationRule);

            distinctValuesProvider.Received().GetDistinctValues(tableName, columnName);
        }

        [Fact]
        public void GenerateValueStringColumnExistingValuesOverrideRule()
        {
            _randomGenerator.GetRandomInteger(Arg.Any<int>(), Arg.Any<int>()).Returns(1);
            var randomizationRule = new ValueRandomizationRule
            {
                ExistingValuesOverride = new object[] { "Two", "Three", "Four" }
            };
            var value = _randomValueProvider.GetRandomValue(_tableName, _distinctColumnName, randomizationRule);

            Assert.Equal(randomizationRule.ExistingValuesOverride[1], value);
        }

        [Fact]
        public void GenerateValueStringColumn()
        {
            const string sampleValue = "string";
            const string allowedCharacters = "ABC";
            const int minStringValueLength = 2;
            const int maxStringValueLength = 9;

            _randomGenerator.GetRandomString(minStringValueLength, maxStringValueLength, allowedCharacters).Returns(sampleValue);
            var randomizationRule = new ValueRandomizationRule
            {
                UseExistingValues = false,
                AllowedCharacters = allowedCharacters,
                MinStringValueLength = minStringValueLength,
                MaxStringValueLength = maxStringValueLength
            };

            var value = _randomValueProvider.GetRandomValue(_tableName, _distinctColumnName, randomizationRule);

            Assert.Equal(sampleValue, value);
        }

        [Fact]
        public void GenerateValueIntegerColumn()
        {
            const int sampleValue = 5;
            const int minNumericValue = 2;
            const int maxNumericValue = 9;
            const string integerColumn = "Count";

            _randomGenerator.GetRandomInteger(minNumericValue, maxNumericValue).Returns(sampleValue);
            var randomizationRule = new ValueRandomizationRule
            {
                UseExistingValues = false,
                MinNumericValue = minNumericValue,
                MaxNumericValue = maxNumericValue
            };

            var value = _randomValueProvider.GetRandomValue(_tableName, integerColumn, randomizationRule);

            Assert.Equal(sampleValue, value);
        }

        [Fact]
        public void GenerateValueDoubleColumn()
        {
            const double sampleValue = 5;
            const int minNumericValue = 2;
            const int maxNumericValue = 9;
            const string doubleColumn = "Price";

            _randomGenerator.GetRandomDouble(minNumericValue, maxNumericValue).Returns(sampleValue);
            var randomizationRule = new ValueRandomizationRule
            {
                UseExistingValues = false,
                MinNumericValue = minNumericValue,
                MaxNumericValue = maxNumericValue
            };

            var value = _randomValueProvider.GetRandomValue(_tableName, doubleColumn, randomizationRule);

            Assert.Equal(sampleValue, value);
        }

        [Fact]
        public void GenerateValueDateTimeColumn()
        {
            DateTime sampleValue = DateTime.Now;
            DateTime minDateTimeValue = DateTime.Now.AddDays(-1);
            DateTime maxDateTimeValue = DateTime.Now;
            const string doubleColumn = "CreatedAt";

            _randomGenerator.GetRandomDateTime(minDateTimeValue, maxDateTimeValue).Returns(sampleValue);
            var randomizationRule = new ValueRandomizationRule
            {
                UseExistingValues = false,
                MinDateTimeValue = minDateTimeValue,
                MaxDateTimeValue = maxDateTimeValue
            };

            var value = _randomValueProvider.GetRandomValue(_tableName, doubleColumn, randomizationRule);

            Assert.Equal(sampleValue, value);
        }

        [Fact]
        public void GenerateValueStringColumnCollection()
        {
            _randomGenerator.GetRandomInteger(Arg.Any<int>(), Arg.Any<int>()).Returns(3);
            var randomizationRule = new ValueRandomizationRule();
            var collection = _randomValueProvider.GetRandomValueCollection(_tableName, _distinctColumnName, randomizationRule);

            Assert.Equal(3, collection.Count());
        }

        [Fact]
        public void GenerateValueStringColumnCollectionNoDuplicates()
        {
            var columnPropertiesProvider = new TableColumnPropertiesProvider(SampleInputs.Table);
            var randomGenerator = new RandomGenerator();
            var distinctValuesProvider = Substitute.For<IDistinctValuesProvider>();
            distinctValuesProvider.GetDistinctValues(_tableName, _distinctColumnName).Returns(_distinctValues);
            var randomValueProvider = new RandomValueProvider(randomGenerator, columnPropertiesProvider, distinctValuesProvider);

            var randomizationRule = new ValueRandomizationRule
            {
                MinCollectionLength = 7,
                MaxCollectionLength = 7
            };

            var collection = randomValueProvider.GetRandomValueCollection(_tableName, _distinctColumnName, randomizationRule);

            var hasDuplicates = collection.GroupBy(v => v).Any(g => g.Count() > 1);
            Assert.False(hasDuplicates);
            Assert.Equal(7, collection.Count());
        }
    }
}
