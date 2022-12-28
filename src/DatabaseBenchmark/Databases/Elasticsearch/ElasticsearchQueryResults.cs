using DatabaseBenchmark.Databases.Common.Interfaces;
using Nest;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public sealed class ElasticsearchQueryResults : IQueryResults
    {
        private readonly ISearchResponse<Dictionary<string, object>> _response;

        private int _documentIndex;

        public IEnumerable<string> ColumnNames => _response.Documents.ElementAt(_documentIndex).Keys;

        public object GetValue(string name) => _response.Documents.ElementAt(_documentIndex)[name];

        public ElasticsearchQueryResults(ISearchResponse<Dictionary<string, object>> response)
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
    }
}
