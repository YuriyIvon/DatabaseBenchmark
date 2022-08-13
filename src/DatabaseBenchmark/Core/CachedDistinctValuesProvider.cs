using DatabaseBenchmark.Core.Interfaces;

namespace DatabaseBenchmark.Core
{
    public class CachedDistinctValuesProvider : IDistinctValuesProvider
    {
        private readonly IDistinctValuesProvider _wrappedProvider;
        private readonly ICache _cache;

        public CachedDistinctValuesProvider(IDistinctValuesProvider wrappedProvider, ICache cache)
        {
            _wrappedProvider = wrappedProvider;
            _cache = cache;
        }

        public object[] GetDistinctValues(string tableName, string columnName)
        {
            var key = string.Join(".", tableName, columnName);
            return _cache.GetOrRead<object[]>(key, () => _wrappedProvider.GetDistinctValues(tableName, columnName));
        }
    }
}
