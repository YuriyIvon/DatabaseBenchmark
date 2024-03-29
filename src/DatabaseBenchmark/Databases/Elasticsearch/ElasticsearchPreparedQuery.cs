﻿using DatabaseBenchmark.Databases.Common.Interfaces;
using Nest;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public sealed class ElasticsearchPreparedQuery : IPreparedQuery
    {
        private readonly ElasticClient _client;
        private readonly SearchRequest _request;

        private ElasticsearchQueryResults _results;

        public IDictionary<string, double> CustomMetrics => null;

        public IQueryResults Results => _results;

        public ElasticsearchPreparedQuery(ElasticClient client, SearchRequest request)
        {
            _client = client;
            _request = request;
        }

        public int Execute()
        {
            var response = _client.Search<Dictionary<string, object>>(_request);
            _results = new ElasticsearchQueryResults(response);

            return 0;
        }

        public void Dispose()
        {
        }
    }
}
