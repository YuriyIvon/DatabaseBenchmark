using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using System.Collections;

namespace DatabaseBenchmark.Databases.AzureSearch
{
    public class AzureSearchValueFormatter : IValueFormatter
    {
        private const string SortableFormat = "yyyy-MM-ddTHH:mm:ss.fffffffZ";

        public string Format(object value) =>
            value switch
            {
                null => "null",
                string => $"'{value.ToString().Replace("'", "''")}'",
                IEnumerable => "null",//throw new InputArgumentException("Collections are not supported in query expressions"),
                DateTime dateTimeValue => dateTimeValue.ToString(SortableFormat),
                DateTimeOffset dateTimeOffsetValue => dateTimeOffsetValue.ToString(SortableFormat),
                bool boolValue => boolValue.ToString().ToLower(),
                _ when value.IsNumber() => value.ToString(),
                _ => $"'{value.ToString().Replace("'", "''")}'"
            };
    }
}
