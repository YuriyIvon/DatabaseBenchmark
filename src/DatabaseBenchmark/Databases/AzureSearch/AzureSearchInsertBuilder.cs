using DatabaseBenchmark.Databases.AzureSearch.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.AzureSearch
{
    public class AzureSearchInsertBuilder : IAzureSearchInsertBuilder
    {
        private readonly Table _table;
        private readonly IDataSourceReader _sourceReader;
        private readonly InsertBuilderOptions _options;

        public int BatchSize => _options.BatchSize;

        public AzureSearchInsertBuilder(
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

            if (!documents.Any())
            {
                throw new NoDataAvailableException();
            }

            return documents;
        }
    }
}