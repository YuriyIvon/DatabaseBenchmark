using Bogus.DataSets;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using GeneratorKind = DatabaseBenchmark.Generators.Options.FinanceGeneratorOptions.GeneratorKind;

namespace DatabaseBenchmark.Generators
{
    public class FinanceGenerator : IGenerator
    {
        private readonly Finance _financeFaker = new();
        private readonly FinanceGeneratorOptions _options;

        public object Current { get; private set; }

        public bool IsBounded => false;

        public FinanceGenerator(FinanceGeneratorOptions options)
        {
            _options = options;
        }

        public bool Next()
        {
            Current = _options.Kind switch
            {
                GeneratorKind.Bic => _financeFaker.Bic(),
                GeneratorKind.Iban => _financeFaker.Iban(),
                GeneratorKind.CreditCardCvv => _financeFaker.CreditCardCvv(),
                GeneratorKind.CreditCardNumber => _financeFaker.CreditCardNumber(),
                GeneratorKind.Currency => _financeFaker.Currency(),
                GeneratorKind.BitcoinAddress => _financeFaker.BitcoinAddress(),
                GeneratorKind.EthereumAddress => _financeFaker.EthereumAddress(),
                _ => throw new InputArgumentException($"Unknown finance generator kind \"{_options.Kind}\"")
            };

            return true;
        }
    }
}
