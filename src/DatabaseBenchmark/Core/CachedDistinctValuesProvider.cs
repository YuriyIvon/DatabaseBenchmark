using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Model;

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

        public object[] GetDistinctValues(string tableName, IValueDefinition column, bool unfoldArray)
        {
            var key = string.Join(".", tableName, column.Name);
            return _cache.GetOrRead<object[]>(key, () => _wrappedProvider.GetDistinctValues(tableName, column, unfoldArray));
        }
    }
}
