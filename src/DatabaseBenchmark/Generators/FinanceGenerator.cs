using Bogus;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using GeneratorKind = DatabaseBenchmark.Generators.Options.FinanceGeneratorOptions.GeneratorKind;

namespace DatabaseBenchmark.Generators
{
    public class FinanceGenerator : IGenerator
    {
        private readonly Faker _faker;
        private readonly FinanceGeneratorOptions _options;

        public object Current { get; private set; }

        public FinanceGenerator(Faker faker, FinanceGeneratorOptions options)
        {
            _faker = faker;
            _options = options;
        }

        public bool Next()
        {
            Current = _options.Kind switch
            {
                GeneratorKind.Bic => _faker.Finance.Bic(),
                GeneratorKind.Iban => _faker.Finance.Iban(),
                GeneratorKind.CreditCardCvv => _faker.Finance.CreditCardCvv(),
                GeneratorKind.CreditCardNumber => _faker.Finance.CreditCardNumber(),
                GeneratorKind.Currency => _faker.Finance.Currency(),
                GeneratorKind.BitcoinAddress => _faker.Finance.BitcoinAddress(),
                GeneratorKind.EthereumAddress => _faker.Finance.EthereumAddress(),
                _ => throw new InputArgumentException($"Unknown finance generator kind \"{_options.Kind}\"")
            };

            return true;
        }
    }
}
