using Azure.Search.Documents.Models;
using DatabaseBenchmark.Databases.Common.Interfaces;
using System.Text.Json;

namespace DatabaseBenchmark.Databases.AzureSearch
{
    public sealed class AzureSearchQueryResults : IQueryResults
    {
        private readonly SearchResults<Dictionary<string, object>> _results;
        private SearchResult<Dictionary<string, object>> _currentResult;
        private IEnumerator<SearchResult<Dictionary<string, object>>> _enumerator;

        public IEnumerable<string> ColumnNames => _currentResult?.Document.Keys ?? Enumerable.Empty<string>();

        public object GetValue(string name) => _currentResult?.Document.TryGetValue(name, out var value) == true ? UnwrapJsonValue(value) : null;

        public AzureSearchQueryResults(SearchResults<Dictionary<string, object>> results)
        {
            _results = results;
            _enumerator = _results.GetResults().GetEnumerator();
        }

        public bool Read()
        {
            if (_enumerator.MoveNext())
            {
                _currentResult = _enumerator.Current;
                return true;
            }

            return false;
        }

        private static object UnwrapJsonValue(object value) =>
            value switch
            {
                JsonElement jsonElement => jsonElement.ValueKind switch
                {
                    JsonValueKind.String => jsonElement.GetString(),
                    JsonValueKind.Number => jsonElement.TryGetInt64(out long l) ? l : jsonElement.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    JsonValueKind.Array => jsonElement.EnumerateArray().Select(e => UnwrapJsonValue(e)).ToList(),
                    _ => jsonElement.ToString()
                },
                _ => value
            };
    }
}