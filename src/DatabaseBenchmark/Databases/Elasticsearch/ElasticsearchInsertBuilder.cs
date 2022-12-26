using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Elasticsearch.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public class ElasticsearchInsertBuilder : IElasticsearchInsertBuilder
    {
        private readonly Table _table;
        private readonly IDataSource _source;

        public int BatchSize { get; set; } = 1;

        public ElasticsearchInsertBuilder(Table table, IDataSource source)
        {
            _table = table;
            _source = source;
        }

        public IEnumerable<object> Build()
        {
            var buffer = new List<object>();

            for (int i = 0; i < BatchSize && _source.Read(); i++)
            {
                var document = _table.Columns
                .Where(c => !c.DatabaseGenerated)
                    .ToDictionary(
                        c => c.Name,
                        c => _source.GetValue(c.GetNativeType(), c.Name));

                buffer.Add(document);
            }

            return buffer;
        }
    }
}
