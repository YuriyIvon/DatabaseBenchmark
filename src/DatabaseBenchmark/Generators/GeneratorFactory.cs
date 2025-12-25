using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Plugins.Interfaces;

namespace DatabaseBenchmark.Generators
{
    public class GeneratorFactory : IGeneratorFactory
    {
        private readonly IDatabase _currentDatabase;
        private readonly DataSourceIteratorGeneratorFactory _dataSourceIteratorGeneratorFactory;
        private readonly IPluginRepository _pluginRepository;
        private readonly IGeneratedValuesContext _generatedValuesContext;

        public GeneratorFactory(
            IDataSourceFactory dataSourceFactory,
            IDatabase currentDatabase,
            IPluginRepository pluginRepository,
            IGeneratedValuesContext generatedValuesContext)
        {
            _dataSourceIteratorGeneratorFactory = new DataSourceIteratorGeneratorFactory(dataSourceFactory);
            _currentDatabase = currentDatabase;
            _pluginRepository = pluginRepository;
            _generatedValuesContext = generatedValuesContext;
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
                EmbeddingGeneratorOptions o => new EmbeddingGenerator(o, CreateSourceGenerator(o.SourceGeneratorOptions, nameof(EmbeddingGenerator)), _pluginRepository),
                FinanceGeneratorOptions o => new FinanceGenerator(o),
                FloatGeneratorOptions o => new FloatGenerator(o),
                ColumnItemGeneratorOptions o => new ColumnItemGenerator(o, _currentDatabase),
                ColumnIteratorGeneratorOptions o => new ColumnIteratorGenerator(o, _currentDatabase),
                ColumnReferenceGeneratorOptions o => new ColumnReferenceGenerator(o, _generatedValuesContext),
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
