namespace DatabaseBenchmark.Common
{
    public static class MathExtensions
    {
        public static T Percentile<T>(this IEnumerable<T> input, int percent)
        {
            int index = (input.Count() - 1) * percent / 100;
            return input.OrderBy(x => x).ElementAt(index);
        }
    }
}
