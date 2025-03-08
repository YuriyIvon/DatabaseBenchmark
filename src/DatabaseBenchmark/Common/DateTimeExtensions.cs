namespace DatabaseBenchmark.Common
{
    public static class DateTimeExtensions
    {
        private const string SortableFormat = "yyyy-MM-ddTHH:mm:ss.fffffff";

        public static string ToSortableString(this DateTime dateTime) =>
            dateTime.ToString(SortableFormat);

        public static string ToSortableString(this DateTimeOffset dateTime) =>
            dateTime.ToString(SortableFormat);
    }
}
