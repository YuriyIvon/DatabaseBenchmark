using DatabaseBenchmark.Databases.Common.Interfaces;
using Elastic.Clients.Elasticsearch;
using System.Text.Json;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public sealed class ElasticsearchQueryResults : IQueryResults
    {
        private readonly SearchResponse<Dictionary<string, object>> _response;

        private int _documentIndex = -1;

        public IEnumerable<string> ColumnNames => _response.Documents.ElementAt(_documentIndex).Keys;

        public object GetValue(string name) => UnwrapJsonValue(_response.Documents.ElementAt(_documentIndex)[name]);

        public ElasticsearchQueryResults(SearchResponse<Dictionary<string, object>> response)
        {
            _response = response;
        }

        public bool Read()
        {
            if (_documentIndex < _response.Documents.Count - 1)
            {
                _documentIndex++;
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
