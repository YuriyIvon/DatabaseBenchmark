﻿using DatabaseBenchmark.Databases.Elasticsearch.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Model;
using Nest;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public sealed class ElasticsearchInsertExecutor : IQueryExecutor
    {
        private readonly IElasticClient _client;
        private readonly Table _table;
        private readonly IElasticsearchInsertBuilder _insertBuilder;

        public ElasticsearchInsertExecutor(
            IElasticClient client,
            Table table,
            IElasticsearchInsertBuilder insertBuilder)
        {
            _client = client;
            _table = table;
            _insertBuilder = insertBuilder;
        }

        public IPreparedQuery Prepare()
        {
            var documents = _insertBuilder.Build();

            return new ElasticsearchPreparedInsert(_client, _table, documents);
        }

        public IPreparedQuery Prepare(ITransaction transaction) => Prepare();

        public void Dispose()
        {
        }
    }
}
