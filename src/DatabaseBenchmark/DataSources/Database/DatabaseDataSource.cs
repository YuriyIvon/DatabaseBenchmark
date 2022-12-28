using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Utils;

namespace DatabaseBenchmark.DataSources.Database
{
    public sealed class DatabaseDataSource : IDataSource
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

        public object GetValue(Type type, string name) => _query.Results.GetValue(name);

        public bool Read()
        {
            if (_query == null)
            {
                Open();
            }

            return _query.Results.Read();
        }

        public void Dispose()
        {
            _query?.Dispose();
            _executor?.Dispose();
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
