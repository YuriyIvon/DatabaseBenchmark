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
        private readonly IDatabase _database;
        private readonly DataSourceIteratorGeneratorFactory _dataSourceIteratorGeneratorFactory;

        public GeneratorFactory(IDataSourceFactory dataSourceFactory, IDatabase database)
        {
            _dataSourceIteratorGeneratorFactory = new DataSourceIteratorGeneratorFactory(dataSourceFactory);
            _database = database;
        }

        public IGenerator Create(IGeneratorOptions options)
        {
            IGenerator generator = options switch
            {
                AddressGeneratorOptions o => new AddressGenerator(o),
                BooleanGeneratorOptions o => new BooleanGenerator(o),
                CollectionGeneratorOptions o => new CollectionGenerator(o, CreateSourceGenerator(o.SourceGeneratorOptions, nameof(CollectionGeneratorOptions))),
                CompanyGeneratorOptions o => new CompanyGenerator(o),
                ConstantGeneratorOptions o => new ConstantGenerator(o),
                DataSourceIteratorGeneratorOptions o => _dataSourceIteratorGeneratorFactory.Create(o),
                DateTimeGeneratorOptions o => new DateTimeGenerator(o),
                FinanceGeneratorOptions o => new FinanceGenerator(o),
                FloatGeneratorOptions o => new FloatGenerator(o),
                ColumnItemGeneratorOptions o => new ColumnItemGenerator(o, _database),
                ColumnIteratorGeneratorOptions o => new ColumnIteratorGenerator(o, _database),
                GuidGeneratorOptions => new GuidGenerator(),
                IntegerGeneratorOptions o => new IntegerGenerator(o),
                InternetGeneratorOptions o => new InternetGenerator(o),
                ListItemGeneratorOptions o => new ListItemGenerator(o),
                ListIteratorGeneratorOptions o => new ListIteratorGenerator(o),
                NameGeneratorOptions o => new NameGenerator(o),
                NullGeneratorOptions o => new NullGenerator(o, CreateSourceGenerator(o.SourceGeneratorOptions, nameof(NullGenerator))),
                PatternGeneratorOptions o => new PatternGenerator(o),
                PhoneGeneratorOptions o => new PhoneGenerator(o),
                StringGeneratorOptions o => new StringGenerator(o),
                TextGeneratorOptions o => new TextGenerator(o),
                UniqueGeneratorOptions o => new UniqueGenerator(o, CreateSourceGenerator(o.SourceGeneratorOptions, nameof(UniqueGenerator))),
                VehicleGeneratorOptions o => new VehicleGenerator(o),
                _ => throw new InputArgumentException($"Unknown generator options type \"{options.GetType()}\"")
            };

            return generator;
        }

        private IGenerator CreateSourceGenerator(IGeneratorOptions options, string parentGeneratorName) =>
            options != null ? Create(options) : throw new InputArgumentException($"Source generator options are not specified for {parentGeneratorName}");
    }
}
