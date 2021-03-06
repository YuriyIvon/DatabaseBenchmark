using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Utils;

namespace DatabaseBenchmark.DataSources.Database
{
    public class DatabaseDataSource : IDataSource
    {
        private readonly string _filePath;
        private readonly IDatabaseFactory _databaseFactory;

        private IQueryExecutor _executor;
        private IPreparedQuery _query;

        public DatabaseDataSource(
            string filePath, 
            IDatabaseFactory databaseFactory)
        {
            _filePath = filePath;
            _databaseFactory = databaseFactory;
        }

        public object GetValue(string name) => _query.GetValue(name);

        public bool Read()
        {
            if (_query == null)
            {
                Open();
            }

            return _query.Read();
        }

        public void Dispose()
        {
            if (_query != null)
            {
                _query.Dispose();
            }

            if (_executor != null)
            {
                _executor.Dispose();
            }
        }

        private void Open()
        {
            var options = JsonUtils.DeserializeFile<DatabaseDataSourceOptions>(_filePath);
            var database = _databaseFactory.Create(options.DatabaseType, options.ConnectionString);
            var queryText = File.ReadAllText(options.QueryFilePath);

            var query = new RawQuery
            {
                Text = queryText,
                TableName = options.TableName
            };

            var executorFactory = database.CreateRawQueryExecutorFactory(query);
            _executor = executorFactory.Create();
            _query = _executor.Prepare();
            _query.Execute();
        }
    }
}
