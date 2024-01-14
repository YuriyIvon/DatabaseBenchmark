using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Generators
{
    public class ColumnIteratorGenerator : IGenerator
    {
        private readonly ColumnIteratorGeneratorOptions _options;
        private readonly IDatabase _database;

        private ListIteratorGenerator _itemGenerator = null;

        public object Current => _itemGenerator.Current;

        public ColumnIteratorGenerator(ColumnIteratorGeneratorOptions options, IDatabase database)
        {
            _options = options;
            _database = database;
        }

        public bool Next()
        {
            if (_itemGenerator == null)
            {
                Initialize();
            }

            return _itemGenerator.Next();
        }

        private void Initialize()
        {
            var listIteratorGeneratorOptions = new ListIteratorGeneratorOptions
            {
                Items = ReadKeys()
            };

            _itemGenerator = new ListIteratorGenerator(listIteratorGeneratorOptions);
        }

        //TODO: Make shared between two generators
        private object[] ReadKeys()
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
            var preparedQuery = executor.Prepare();
            preparedQuery.Execute();

            var results = preparedQuery.Results;
            var keys = new List<object>();

            while (results.Read())
            {
                keys.Add(results.GetValue(_options.ColumnName));
            }

            return keys.ToArray();
        }
    }
}
