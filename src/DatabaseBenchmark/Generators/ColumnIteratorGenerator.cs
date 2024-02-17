using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Generators
{
    public class ColumnIteratorGenerator : IGenerator, IDisposable
    {
        private readonly ColumnIteratorGeneratorOptions _options;
        private readonly IDatabase _database;

        private IPreparedQuery _query;

        public object Current => _query.Results.GetValue(_options.ColumnName);

        public ColumnIteratorGenerator(ColumnIteratorGeneratorOptions options, IDatabase database)
        {
            _options = options;
            _database = database;
        }

        public bool Next()
        {
            if (_query == null)
            {
                Initialize();
            }

            return _query.Results.Read();
        }

        public void Dispose() => _query.Dispose();

        //TODO: Make shared between two generators
        private void Initialize()
        {
            var table = new Table
            {
                Name = _options.TableName,
                Columns =
                [
                    new()
                    {
                        Name = _options.ColumnName,
                        Type = _options.ColumnType
                    }
                ]
            };

            var query = new Query
            {
                Columns = [_options.ColumnName],
                Distinct = _options.Distinct
            };

            var executorFactory = _database.CreateQueryExecutorFactory(table, query);
            var executor = executorFactory.Create();
            _query = executor.Prepare();
            _query.Execute();
        }
    }
}
