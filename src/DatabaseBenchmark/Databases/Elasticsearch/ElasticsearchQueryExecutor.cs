﻿using DatabaseBenchmark.Databases.Elasticsearch.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using Nest;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public sealed class ElasticsearchQueryExecutor : IQueryExecutor
    {
        private readonly ElasticClient _client;
        private readonly IElasticsearchQueryBuilder _queryBuilder;

        public ElasticsearchQueryExecutor(
            ElasticClient client,
            IElasticsearchQueryBuilder queryBuilder)
        {
            _client = client;
            _queryBuilder = queryBuilder;
        }

        public IPreparedQuery Prepare()
        {
            var request = _queryBuilder.Build();

            return new ElasticsearchPreparedQuery(_client, request);
        }

        public IPreparedQuery Prepare(ITransaction transaction) => Prepare();

        public void Dispose()
        {
        }
    }
}
