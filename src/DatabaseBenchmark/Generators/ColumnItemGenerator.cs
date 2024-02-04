﻿using Bogus;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Generators
{
    public class ColumnItemGenerator : IGenerator, ICollectionGenerator
    {
        private readonly Faker _faker;
        private readonly ColumnItemGeneratorOptions _options;
        private readonly IDatabase _database;

        private ListItemGenerator _itemGenerator = null;

        public object Current => _itemGenerator.Current;

        public IEnumerable<object> CurrentCollection => _itemGenerator.CurrentCollection;

        public ColumnItemGenerator(Faker faker, ColumnItemGeneratorOptions options, IDatabase database)
        {
            _faker = faker;
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

        public bool NextCollection(int length)
        {
            if (_itemGenerator == null)
            {
                Initialize();
            }

            return _itemGenerator.NextCollection(length);
        }

        private void Initialize()
        {
            var listItemGeneratorOptions = new ListItemGeneratorOptions
            {
                Items = ReadKeys(),
                WeightedItems = _options.WeightedItems
            };

            _itemGenerator = new ListItemGenerator(_faker, listItemGeneratorOptions);
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