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
                    GeneratorType.Address => optionsJson.Deserialize<AddressGeneratorOptions>(options),
                    GeneratorType.Boolean => optionsJson.Deserialize<BooleanGeneratorOptions>(options),
                    GeneratorType.Company => optionsJson.Deserialize<CompanyGeneratorOptions>(options),
                    GeneratorType.DataSourceIterator => optionsJson.Deserialize<DataSourceIteratorGeneratorOptions>(options),
                    GeneratorType.DateTime => optionsJson.Deserialize<DateTimeGeneratorOptions>(options),
                    GeneratorType.Finance => optionsJson.Deserialize<FinanceGeneratorOptions>(options),
                    GeneratorType.Float => optionsJson.Deserialize<FloatGeneratorOptions>(options),
                    GeneratorType.ColumnItem => optionsJson.Deserialize<ColumnItemGeneratorOptions>(options),
                    GeneratorType.ColumnIterator => optionsJson.Deserialize<ColumnIteratorGeneratorOptions>(options),
                    GeneratorType.Guid => optionsJson.Deserialize<GuidGeneratorOptions>(options),
                    GeneratorType.Integer => optionsJson.Deserialize<IntegerGeneratorOptions>(options),
                    GeneratorType.Internet => optionsJson.Deserialize<InternetGeneratorOptions>(options),
                    GeneratorType.ListItem => optionsJson.Deserialize<ListItemGeneratorOptions>(options),
                    GeneratorType.ListIterator => optionsJson.Deserialize<ListIteratorGeneratorOptions>(options),
                    GeneratorType.Name => optionsJson.Deserialize<NameGeneratorOptions>(options),
                    GeneratorType.Null => optionsJson.Deserialize<NullGeneratorOptions>(options),
                    GeneratorType.Phone => optionsJson.Deserialize<PhoneGeneratorOptions>(options),
                    GeneratorType.String => optionsJson.Deserialize<StringGeneratorOptions>(options),
                    GeneratorType.Text => optionsJson.Deserialize<TextGeneratorOptions>(options),
                    GeneratorType.Unique => optionsJson.Deserialize<UniqueGeneratorOptions>(options),
                    GeneratorType.Vehicle => optionsJson.Deserialize<VehicleGeneratorOptions>(options),
                    _ => throw new InputArgumentException($"Unknown generator type \"{generatorType}\"")
                };
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, IGeneratorOptions value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }
}
