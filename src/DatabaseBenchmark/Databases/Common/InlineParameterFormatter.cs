using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Databases.Common
{
    public static class InlineParameterFormatter
    {
        public static string Format(string format, object value) =>
            value switch
            {
                null => null,
                bool b => b.ToString().ToLower(),
                int n => n.ToString(format),
                long n => n.ToString(format),
                double n => n.ToString(format),
                DateTime dt => dt.ToString(format ?? "s"),
                Guid g => g.ToString(format),
                _ => value.ToString()
            };
    }
}
