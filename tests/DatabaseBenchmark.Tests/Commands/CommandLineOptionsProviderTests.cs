using DatabaseBenchmark.Commands;
using DatabaseBenchmark.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DatabaseBenchmark.Tests.Commands
{
    public class CommandLineOptionsProviderTests
    {
        [Fact]
        public void GetOptions()
        {
            var args = new string[]
            {
                "--BooleanProperty=true",
                "--IntegerProperty=10",
                "--DoubleProperty=10.1",
                "--DateProperty=2007-10-10",
                "--DateTimeProperty=2007-10-10T12:12:12Z",
                "--StringProperty=string"
            };

            var optionsProvider = new CommandLineOptionsProvider(args);
            var options = optionsProvider.GetOptions<Options>();

            Assert.True(options.BooleanProperty);
            Assert.Equal(10, options.IntegerProperty);
            Assert.Equal(10.1, options.DoubleProperty);
            Assert.Equal(DateTime.Parse("2007-10-10"), options.DateProperty);
            Assert.Equal(DateTime.Parse("2007-10-10T12:12:12Z"), options.DateTimeProperty);
            Assert.Equal("string", options.StringProperty);
        }

        [Fact]
        public void GetOptionsMissingRequired()
        {
            var args = Array.Empty<string>();
            var optionsProvider = new CommandLineOptionsProvider(args);

            Assert.Throws<InputArgumentException>(() => optionsProvider.GetOptions<Options>());
        }

        [Fact]
        public void GetPrefixedOptions()
        {
            var args = new string[]
            {
                "--Prefix.StringProperty=string1",
                "--StringProperty=string2"
            };

            var optionsProvider = new CommandLineOptionsProvider(args);
            var options = optionsProvider.GetOptions<PrefixedOptions>();

            Assert.Equal("string1", options.StringProperty);
        }

        private class Options
        {
            [Option("")]
            public bool? BooleanProperty { get; set; }

            [Option("")]
            public int? IntegerProperty { get; set; }

            [Option("")]
            public double? DoubleProperty { get; set; }

            [Option("")]
            public DateTime? DateProperty { get; set; }

            [Option("")]
            public DateTime? DateTimeProperty { get; set; }

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
