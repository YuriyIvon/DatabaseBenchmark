﻿using Bogus;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Generators
{
    public class GeneratorFactory : IGeneratorFactory
    {
        private readonly Faker _faker;
        private readonly IDatabase _database;
        private readonly DataSourceIteratorGeneratorFactory _dataSourceIteratorGeneratorFactory;

        public GeneratorFactory(IDataSourceFactory dataSourceFactory, IDatabase database)
        {
            _dataSourceIteratorGeneratorFactory = new DataSourceIteratorGeneratorFactory(dataSourceFactory);
            _database = database;

            //TODO: pass locale from outside
            _faker = new Faker();
        }

        public IGenerator Create(IGeneratorOptions options)
        {
            IGenerator generator = options switch
            {
                AddressGeneratorOptions o => new AddressGenerator(_faker, o),
                BooleanGeneratorOptions o => new BooleanGenerator(_faker, o),
                CompanyGeneratorOptions o => new CompanyGenerator(_faker, o),
                DataSourceIteratorGeneratorOptions o => _dataSourceIteratorGeneratorFactory.Create(o),
                DateTimeGeneratorOptions o => new DateTimeGenerator(_faker, o),
                FinanceGeneratorOptions o => new FinanceGenerator(_faker, o),
                FloatGeneratorOptions o => new FloatGenerator(_faker, o),
                ColumnItemGeneratorOptions o => new ColumnItemGenerator(_faker, o, _database),
                ColumnIteratorGeneratorOptions o => new ColumnIteratorGenerator(o, _database),
                GuidGeneratorOptions => new GuidGenerator(_faker),
                IntegerGeneratorOptions o => new IntegerGenerator(_faker, o),
                InternetGeneratorOptions o => new InternetGenerator(_faker, o),
                ListItemGeneratorOptions o => new ListItemGenerator(_faker, o),
                ListIteratorGeneratorOptions o => new ListIteratorGenerator(o),
                NameGeneratorOptions o => new NameGenerator(_faker, o),
                NullGeneratorOptions o => new NullGenerator(_faker, o, Create(o.SourceGeneratorOptions)),
                PhoneGeneratorOptions o => new PhoneGenerator(_faker, o),
                StringGeneratorOptions o => new StringGenerator(_faker, o),
                TextGeneratorOptions o => new TextGenerator(_faker, o),
                VehicleGeneratorOptions o => new VehicleGenerator(_faker, o),
                _ => throw new InputArgumentException($"Unknown generator options type \"{options.GetType()}\"")
            };

            return generator;
        }
    }
}
