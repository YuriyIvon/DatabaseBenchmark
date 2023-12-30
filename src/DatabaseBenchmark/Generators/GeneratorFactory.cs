using Bogus;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.Generators
{
    public class GeneratorFactory : IGeneratorFactory
    {
        private readonly Faker _faker = new();
        private readonly IDatabase _database;

        public GeneratorFactory(IDatabase database)
        {
            _database = database;
        }

        public IGenerator Create(GeneratorType type, IGeneratorOptions options) =>
            type switch
            {
                GeneratorType.Address => new AddressGenerator(_faker, (AddressGeneratorOptions)options),
                GeneratorType.Boolean => new BooleanGenerator(_faker, (BooleanGeneratorOptions)options),
                GeneratorType.Company => new CompanyGenerator(_faker, (CompanyGeneratorOptions)options),
                GeneratorType.DateTime => new DateTimeGenerator(_faker, (DateTimeGeneratorOptions)options),
                GeneratorType.Finance => new FinanceGenerator(_faker, (FinanceGeneratorOptions)options),
                GeneratorType.Float => new FloatGenerator(_faker, (FloatGeneratorOptions)options),
                GeneratorType.ForeignColumn => new ForeignColumnGenerator(_faker, (ForeignColumnGeneratorOptions)options, _database),
                GeneratorType.Guid => new GuidGenerator(_faker),
                GeneratorType.Integer => new IntegerGenerator(_faker, (IntegerGeneratorOptions)options),
                GeneratorType.Internet => new InternetGenerator(_faker, (InternetGeneratorOptions)options),
                GeneratorType.ListItem => new ListItemGenerator(_faker, (ListItemGeneratorOptions)options),
                GeneratorType.Name => new NameGenerator(_faker, (NameGeneratorOptions)options),
                GeneratorType.Phone => new PhoneGenerator(_faker, (PhoneGeneratorOptions)options),
                GeneratorType.String => new StringGenerator(_faker, (StringGeneratorOptions)options),
                GeneratorType.Text => new TextGenerator(_faker, (TextGeneratorOptions)options),
                GeneratorType.Vehicle => new VehicleGenerator(_faker, (VehicleGeneratorOptions)options),
                _ => throw new InputArgumentException($"Unknown generator type \"{type}\"")
            };
    }
}
