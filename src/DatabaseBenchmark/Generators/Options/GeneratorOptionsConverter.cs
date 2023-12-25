using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
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

                if (!optionsJson.RootElement.TryGetProperty(nameof(IGeneratorOptions.Type), out var generatorTypeString))
                {
                    throw new InputArgumentException("Property \"Type\" not found in the generator options");
                }

                var generatorType = (GeneratorType)Enum.Parse(typeof(GeneratorType), generatorTypeString.GetString());

                return generatorType switch
                {
                    GeneratorType.Address => optionsJson.RootElement.Deserialize<AddressGeneratorOptions>(),
                    GeneratorType.Boolean => optionsJson.RootElement.Deserialize<BooleanGeneratorOptions>(),
                    GeneratorType.Company => optionsJson.RootElement.Deserialize<CompanyGeneratorOptions>(),
                    GeneratorType.DateTime => optionsJson.RootElement.Deserialize<DateTimeGeneratorOptions>(),
                    GeneratorType.Float => optionsJson.RootElement.Deserialize<FloatGeneratorOptions>(),
                    GeneratorType.ForeignKey => optionsJson.RootElement.Deserialize<ForeignKeyGeneratorOptions>(),
                    GeneratorType.Guid => optionsJson.RootElement.Deserialize<GuidGeneratorOptions>(),
                    GeneratorType.Integer => optionsJson.RootElement.Deserialize<IntegerGeneratorOptions>(),
                    GeneratorType.Internet => optionsJson.RootElement.Deserialize<InternetGeneratorOptions>(),
                    GeneratorType.ListItem => optionsJson.RootElement.Deserialize<ListItemGeneratorOptions>(),
                    GeneratorType.Name => optionsJson.RootElement.Deserialize<NameGeneratorOptions>(),
                    GeneratorType.Phone => optionsJson.RootElement.Deserialize<PhoneGeneratorOptions>(),
                    GeneratorType.String => optionsJson.RootElement.Deserialize<StringGeneratorOptions>(),
                    GeneratorType.Text => optionsJson.RootElement.Deserialize<TextGeneratorOptions>(),
                    GeneratorType.Vehicle => optionsJson.RootElement.Deserialize<VehicleGeneratorOptions>(),
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
