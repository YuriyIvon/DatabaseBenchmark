using Castle.Components.DictionaryAdapter;
using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using System;
using System.Collections.Generic;
using Xunit;

namespace DatabaseBenchmark.Tests.Core
{
    public class ValueFormatterTests
    {
        private readonly IValueFormatter _valueFormatter = new ValueFormatter();

        public static IEnumerable<object[]> FormatSamples =>
        [
            [(byte)123, "123"],
            [(short)123, "123"],
            [(int)123, "123"],
            [(long)123, "123"],
            [(float)1.2345, "1.23"],
            [(double)1.2345, "1.23"],
            [(decimal)1.2345, "1.23"],
            [true, "True"],
            ["abc", "\"abc\""],
            [new Guid("6971bc34-4117-4dba-a405-d2ffa82bf6d2"), "\"6971bc34-4117-4dba-a405-d2ffa82bf6d2\""],
            [new string[] {"one", "two", "three"}, "[\"one\", \"two\", \"three\"]"],
            [new double[] {1.765, 12.351}, "[1.77, 12.35]"],
            [new DateTime(2023, 12, 11), "2023-12-11T00:00:00.0000000"],
            [new DateTime(2023, 12, 11, 1, 2, 3), "2023-12-11T01:02:03.0000000"],
            [null, "NULL"]
        ];

        [Theory]
        [MemberData(nameof(FormatSamples))]
        public void FormatValue(object value, string formattedValue)
        {
            Assert.Equal(formattedValue, _valueFormatter.Format(value));
        }
    }
}
