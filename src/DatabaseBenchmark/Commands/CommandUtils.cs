namespace DatabaseBenchmark.Commands
{
    public static class CommandUtils
    {
        public static IEnumerable<T> FilterByIndexes<T>(IEnumerable<T> collection, string indexString)
        {
            if (!string.IsNullOrEmpty(indexString))
            {
                int[] querySecnarioItemIndexes = indexString
                    .Split(',', StringSplitOptions.TrimEntries)
                    .Select(int.Parse)
                    .ToArray();

                return collection.Where((_, i) => querySecnarioItemIndexes.Contains(i + 1));
            }

            return collection;
        }
    }
}
