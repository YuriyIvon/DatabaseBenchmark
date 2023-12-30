using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using System.Text.Json;

namespace DatabaseBenchmark.Generators.Options
{
    public static class GeneratorOptionsDeserializer
    {
        public static IGeneratorOptions Deserialize(JsonElement optionsJson)
        {
            if (optionsJson.ValueKind == JsonValueKind.Undefined)
            {
                return null;
            }

            if (!optionsJson.TryGetProperty(nameof(IGeneratorOptions.Type), out var generatorTypeString))
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
    }
}
