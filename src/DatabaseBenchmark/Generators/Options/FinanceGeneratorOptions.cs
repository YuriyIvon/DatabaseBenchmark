using DatabaseBenchmark.Generators.Interfaces;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    public class FinanceGeneratorOptions : IGeneratorOptions
    {
        public GeneratorType Type => GeneratorType.Finance;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GeneratorKind Kind { get; set; }

        public enum GeneratorKind
        {
            Bic,
            BitcoinAddress,
            CreditCardCvv,
            CreditCardNumber,
            Currency,
            EthereumAddress,
            Iban
        }
    }
}
