using DatabaseBenchmark.Core.Interfaces;

namespace DatabaseBenchmark.Core
{
    public class MemoryCache : ICache
    {
        private readonly object _syncRoot = new();
        private readonly Dictionary<string, object> _entries = new();

        public T GetOrRead<T>(string key, Func<T> reader)
        {
            lock (_syncRoot)
            {
                if (!_entries.TryGetValue(key, out var value))
                {
                    value = reader();
                    _entries.Add(key, value);
                }

                return (T)value;
            }
        }
    }
}
