using DatabaseBenchmark.Databases.Interfaces;
using Nest;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public class ElasticsearchPreparedQuery : IPreparedQuery
    {
        private readonly ElasticClient _client;
        private readonly SearchRequest _request;

        private ISearchResponse<Dictionary<string, object>> _result;
        private int _documentIndex;

        public IEnumerable<string> ColumnNames => _result.Documents.ElementAt(_documentIndex).Keys;

        public IDictionary<string, double> CustomMetrics => null;

        public ElasticsearchPreparedQuery(ElasticClient client, SearchRequest request)
        {
            _client = client;
            _request = request;
        }

        public void Execute()
        {
            _result = _client.Search<Dictionary<string, object>>(_request);
        }

        public object GetValue(string name) => _result.Documents.ElementAt(_documentIndex)[name];

        public bool Read()
        {
            if (_documentIndex < _result.Documents.Count - 1)
            {
                _documentIndex++;
                return true;
            }

            return false;
        }

        public void Dispose()
        {
        }
    }
}
