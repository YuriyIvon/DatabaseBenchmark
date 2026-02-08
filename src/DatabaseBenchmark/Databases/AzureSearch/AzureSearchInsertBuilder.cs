using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.AzureSearch.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Model;
using System.Text;

namespace DatabaseBenchmark.Databases.AzureSearch
{
    public class AzureSearchInsertBuilder : IAzureSearchInsertBuilder
    {
        private readonly Table _table;
        private readonly IDataSourceReader _sourceReader;
        private readonly InsertBuilderOptions _options;
        private readonly AzureSearchInsertOptions _insertOptions;
        private readonly string _primaryKeyColumn;

        public int BatchSize => _options.BatchSize;

        public AzureSearchInsertBuilder(
            Table table,
            IDataSourceReader sourceReader,
            IOptionsProvider optionsProvider,
            InsertBuilderOptions options)
        {
            _table = table;
            _sourceReader = sourceReader;
            _options = options;
            _insertOptions = optionsProvider.GetOptions<AzureSearchInsertOptions>();

            _primaryKeyColumn = table.Columns.FirstOrDefault(c => c.PrimaryKey)?.Name;
        }

        public IEnumerable<object> Build()
        {
            var documents = new List<object>();

            for (int i = 0; i < BatchSize && _sourceReader.ReadDictionary(_table.Columns, out var document); i++)
            {
                if (_insertOptions.Base64EncodePrimaryKey && _primaryKeyColumn != null)
                {
                    Base64EncodePrimaryKey(document);
                }

                documents.Add(document);
            }

            if (!documents.Any())
            {
                throw new NoDataAvailableException();
            }

            return documents;
        }

        private void Base64EncodePrimaryKey(IDictionary<string, object> document)
        {
            if (document.TryGetValue(_primaryKeyColumn, out var keyValue) && keyValue != null)
            {
                var keyString = keyValue.ToString();
                var keyBytes = Encoding.UTF8.GetBytes(keyString);
                var keyBase64 = Convert.ToBase64String(keyBytes).Replace('+', '-').Replace('/', '_');

                document[_primaryKeyColumn] = keyBase64;
            }
        }
    }
}