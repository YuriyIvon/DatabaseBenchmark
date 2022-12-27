using DatabaseBenchmark.Databases.Elasticsearch.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public class ElasticsearchInsertBuilder : IElasticsearchInsertBuilder
    {
        private readonly Table _table;
        private readonly IDataSourceReader _sourceReader;

        public int BatchSize { get; set; } = 1;

        public ElasticsearchInsertBuilder(Table table, IDataSourceReader sourceReader)
        {
            _table = table;
            _sourceReader = sourceReader;
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
