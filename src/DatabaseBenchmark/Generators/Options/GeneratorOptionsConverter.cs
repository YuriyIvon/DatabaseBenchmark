using DatabaseBenchmark.Common;
using DatabaseBenchmark.Model;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    public class GeneratorOptionsConverter : JsonConverter<IGeneratorOptions>
    {
        public override IGeneratorOptions Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                using var optionsJson = JsonDocument.ParseValue(ref reader);

                if (!optionsJson.RootElement.TryGetProperty(nameof(GeneratorOptionsBase.Type), out var generatorTypeString))
                {
                    throw new InputArgumentException("Property \"Type\" not found in the generator options");
                }

                var generatorType = (GeneratorType)Enum.Parse(typeof(GeneratorType), generatorTypeString.GetString());

                return generatorType switch
                {
                    GeneratorType.Address => optionsJson.Deserialize<AddressGeneratorOptions>(),
                    GeneratorType.Boolean => optionsJson.Deserialize<BooleanGeneratorOptions>(),
                    GeneratorType.Company => optionsJson.Deserialize<CompanyGeneratorOptions>(),
                    GeneratorType.DateTime => optionsJson.Deserialize<DateTimeGeneratorOptions>(),
                    GeneratorType.Finance => optionsJson.Deserialize<FinanceGeneratorOptions>(),
                    GeneratorType.Float => optionsJson.Deserialize<FloatGeneratorOptions>(),
                    GeneratorType.ForeignColumn => optionsJson.Deserialize<ForeignColumnGeneratorOptions>(),
                    GeneratorType.Guid => optionsJson.Deserialize<GuidGeneratorOptions>(),
                    GeneratorType.Integer => optionsJson.Deserialize<IntegerGeneratorOptions>(),
                    GeneratorType.Internet => optionsJson.Deserialize<InternetGeneratorOptions>(),
                    GeneratorType.ListItem => optionsJson.Deserialize<ListItemGeneratorOptions>(),
                    GeneratorType.Name => optionsJson.Deserialize<NameGeneratorOptions>(),
                    GeneratorType.Phone => optionsJson.Deserialize<PhoneGeneratorOptions>(),
                    GeneratorType.String => optionsJson.Deserialize<StringGeneratorOptions>(),
                    GeneratorType.Text => optionsJson.Deserialize<TextGeneratorOptions>(),
                    GeneratorType.Vehicle => optionsJson.Deserialize<VehicleGeneratorOptions>(),
                    _ => throw new InputArgumentException($"Unknown generator type \"{generatorType}\"")
                };
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, IGeneratorOptions value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
