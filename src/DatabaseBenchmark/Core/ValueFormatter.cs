using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using System.Collections;

namespace DatabaseBenchmark.Core
{
    public class ValueFormatter : IValueFormatter
    {
        public string Format(object value) =>
            value switch
            {
                string => $"\"{value}\"",
                IEnumerable arrayValue => $"[{string.Join(", ", arrayValue.Cast<object>().Select(Format))}]",
                DateTime dateTimeValue => dateTimeValue.ToSortableString(), // TODO: Make output date/time format configurable
                DateTimeOffset dateTimeOffsetValue => dateTimeOffsetValue.ToSortableString(), // TODO: Make output date/time format configurable
                double or float => string.Format($"{value:0.##}"), // TODO: Make precision configurable
                bool boolValue => boolValue.ToString(),
                _ when value.IsNumber() => value.ToString(),
                _ => $"\"{value}\"" // TODO: Make quotemarks configurable
            };
    }
}
