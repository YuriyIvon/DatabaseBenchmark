﻿using Bogus;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Generators
{
    public class ForeignColumnGenerator : IGenerator, ICollectionGenerator
    {
        private readonly Faker _faker;
        private readonly ForeignColumnGeneratorOptions _options;
        private readonly IDatabase _database;

        private ListItemGenerator _itemGenerator = null;

        public ForeignColumnGenerator(Faker faker, ForeignColumnGeneratorOptions options, IDatabase database)
        {
            _faker = faker;
            _options = options;
            _database = database;
        }

        public object Generate()
        {
            if (_itemGenerator == null)
            {
                Initialize();
            }

            return _itemGenerator.Generate();
        }

        public IEnumerable<object> GenerateCollection(int length)
        {
            if (_itemGenerator == null)
            {
                Initialize();
            }

            return _itemGenerator.GenerateCollection(length);
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