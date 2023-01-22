using DatabaseBenchmark.Commands;
using DatabaseBenchmark.Common;
using System;
using Xunit;

namespace DatabaseBenchmark.Tests.Commands
{
    public class JsonOptionsProviderTests
    {
        [Fact]
        public void GetOptions()
        {
            var json = @"{
  ""BooleanProperty"": true,
  ""IntegerProperty"": 10,
  ""DoubleProperty"": 10.1,
  ""DateProperty"": ""2007-10-10"",
  ""DateTimeProperty"": ""2007-10-10T12:12:12Z"",
  ""StringArrayProperty"": [""one"", ""two"", ""three""],
  ""IntArrayProperty"": [1, 2, 3, 4],
  ""StringProperty"": ""string""
}";
            var optionsProvider = new JsonOptionsProvider(json);
            var options = optionsProvider.GetOptions<Options>();

            Assert.True(options.BooleanProperty);
            Assert.Equal(10, options.IntegerProperty);
            Assert.Equal(10.1, options.DoubleProperty);
            Assert.Equal(DateTime.Parse("2007-10-10"), options.DateProperty);
            Assert.Equal(DateTime.Parse("2007-10-10T12:12:12Z"), options.DateTimeProperty);
            Assert.Equal(new[] { "one", "two", "three" }, options.StringArrayProperty);
            Assert.Equal(new[] { 1, 2, 3, 4 }, options.IntArrayProperty);
            Assert.Equal("string", options.StringProperty);
        }

        [Fact]
        public void GetPrefixedOptions()
        {
            var json = @"{
  ""Prefix.StringProperty"": ""string1"",
  ""StringProperty"": ""string2""
}";
            var optionsProvider = new JsonOptionsProvider(json);
            var options = optionsProvider.GetOptions<PrefixedOptions>();

            Assert.Equal("string1", options.StringProperty);
        }

        [Fact]
        public void GetOptionsMissingRequired()
        {
            var json = @"{}";
            var optionsProvider = new JsonOptionsProvider(json);
            
            Assert.Throws<InputArgumentException>(() => optionsProvider.GetOptions<Options>());
        }

        [Fact]
        public void GetOptionsWithParameters()
        {
            var json = @"{
  ""DateProperty"": ""2007-10-10"",
  ""DateTimeProperty"": ""2007-10-10T12:12:12Z"",
  ""StringProperty"": ""${StringPropertyPrefix} string""
}";
            var parametersJson = @"{
  ""BooleanProperty"": true,
  ""IntegerProperty"": 10,
  ""DoubleProperty"": 10.1,
  ""DateTimeProperty"": ""2008-10-10T12:12:12Z"",
  ""StringPropertyPrefix"": ""prefix""
}";
            var optionsProvider = new JsonOptionsProvider(json, parametersJson);
            var options = optionsProvider.GetOptions<Options>();

            Assert.True(options.BooleanProperty);
            Assert.Equal(10, options.IntegerProperty);
            Assert.Equal(10.1, options.DoubleProperty);
            Assert.Equal(DateTime.Parse("2007-10-10"), options.DateProperty);
            Assert.Equal(DateTime.Parse("2007-10-10T12:12:12Z"), options.DateTimeProperty);
            Assert.Equal("prefix string", options.StringProperty);
        }

        private interface IOptions
        {
            [Option("")]
            public bool? BooleanProperty { get; set; }

            [Option("")]
            public int? IntegerProperty { get; set; }
        }

        private class Options : IOptions
        {
            public bool? BooleanProperty { get; set; }

            public int? IntegerProperty { get; set; }

            [Option("")]
            public double? DoubleProperty { get; set; }

            [Option("")]
            public DateTime? DateProperty { get; set; }

            [Option("")]
            public DateTime? DateTimeProperty { get; set; }

            [Option("")]
            public string[] StringArrayProperty { get; set; }

            [Option("")]
            public int[] IntArrayProperty { get; set; }

            [Option("", true)]
            public string StringProperty { get; set; }
        }

        [OptionPrefix("Prefix")]
        private class PrefixedOptions
        {
            [Option("")]
            public string StringProperty { get; set; }
        }
    }
}
