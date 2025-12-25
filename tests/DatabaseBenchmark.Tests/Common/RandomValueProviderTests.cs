using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Tests.Utils;
using NSubstitute;
using System;
using System.Linq;
using Xunit;

namespace DatabaseBenchmark.Tests.Common
{
    public class RandomValueProviderTests
    {
        private readonly Table _table;
        private readonly Column _distinctColumn;
        private readonly object[] _distinctValues;
        private readonly IGeneratorFactory _randomGeneratorFactory;
        private readonly IRandomValueProvider _randomValueProvider;

        public RandomValueProviderTests()
        {
            _table = SampleInputs.Table;
            _distinctColumn = _table.Columns.Single(c => c.Name == "Category");
            _distinctValues = ["One", "Two", "Three", "Four", "Five", "Six", "Seven"];

            _randomGeneratorFactory = new GeneratorFactory(null, null, null, null);
            var distinctValuesProvider = Substitute.For<IDistinctValuesProvider>();
            distinctValuesProvider.GetDistinctValues(_table.Name, _distinctColumn, false).Returns(_distinctValues);
            _randomValueProvider = new RandomValueProvider(_randomGeneratorFactory, distinctValuesProvider);
        }

        [Fact]
        public void GenerateValueStringColumnDefaultRule()
        {
            var randomizationRule = new ValueRandomizationRule();
            var value = _randomValueProvider.GetValue(_table.Name, _distinctColumn, randomizationRule);

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

            var value = _randomValueProvider.GetValue(_table.Name, _distinctColumn, randomizationRule);

            Assert.IsType<string>(value);
        }

        [Fact]
        public void GenerateValueIntegerColumn()
        {
            Column integerColumn = _table.Columns.Single(c => c.Name == "Count");

            var randomizationRule = new ValueRandomizationRule
            {
                UseExistingValues = false,
                GeneratorOptions = new IntegerGeneratorOptions()
            };

            var value = _randomValueProvider.GetValue(_table.Name, integerColumn, randomizationRule);

            Assert.IsType<int>(value);
        }

        [Fact]
        public void GenerateValueDoubleColumn()
        {
            Column doubleColumn = _table.Columns.Single(c => c.Name == "Price");

            var randomizationRule = new ValueRandomizationRule
            {
                UseExistingValues = false,
                GeneratorOptions = new FloatGeneratorOptions()
            };

            var value = _randomValueProvider.GetValue(_table.Name, doubleColumn, randomizationRule);

            Assert.IsType<double>(value);
        }

        [Fact]
        public void GenerateValueDateTimeColumnDefaultRule()
        {
            Column dateTimeColumn = _table.Columns.Single(c => c.Name == "CreatedAt");

            var randomizationRule = new ValueRandomizationRule
            {
                UseExistingValues = false,
                GeneratorOptions = new DateTimeGeneratorOptions()
            };

            var value = _randomValueProvider.GetValue(_table.Name, dateTimeColumn, randomizationRule);

            Assert.IsType<DateTime>(value);
        }

        [Fact]
        public void GenerateValueNoDataAvailable()
        {
            var randomizationRule = new ValueRandomizationRule
            {
                UseExistingValues = false,
                GeneratorOptions = new ListIteratorGeneratorOptions
                {
                    Items = []
                }
            };

            Assert.Throws<NoDataAvailableException>(() => _randomValueProvider.GetValue(_table.Name, _distinctColumn, randomizationRule));
            Assert.Throws<NoDataAvailableException>(() => _randomValueProvider.Next());
        }
    }
}
