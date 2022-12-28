using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Elasticsearch.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public class ElasticsearchInsertBuilder : IElasticsearchInsertBuilder
    {
        private readonly Table _table;
        private readonly IDataSourceReader _sourceReader;
        private readonly InsertBuilderOptions _options;

        public int BatchSize => _options.BatchSize;

        public ElasticsearchInsertBuilder(
            Table table,
            IDataSourceReader sourceReader,
            InsertBuilderOptions options)
        {
            _table = table;
            _sourceReader = sourceReader;
            _options = options;
        }

        public IEnumerable<object> Build()
        {
            var documents = new List<object>();

            for (int i = 0; i < BatchSize && _sourceReader.ReadDictionary(_table.Columns, out var document); i++)
            {
                documents.Add(document);
            }

            return documents;
        }
    }
}
