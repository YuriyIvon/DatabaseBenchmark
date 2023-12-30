using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    public class FinanceGeneratorOptions : GeneratorOptionsBase
    {
        public override GeneratorType Type => GeneratorType.Finance;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GeneratorKind Kind { get; set; } = GeneratorKind.Iban;

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
