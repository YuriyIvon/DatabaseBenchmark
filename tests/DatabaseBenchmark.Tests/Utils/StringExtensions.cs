using System.Text.RegularExpressions;

namespace DatabaseBenchmark.Tests.Utils
{
    public static class StringExtensions
    {
        public static string NormalizeSpaces(this string text) =>
            Regex.Replace(text.ReplaceLineEndings(" "), @"\s+", " ").Trim();

    }
}
