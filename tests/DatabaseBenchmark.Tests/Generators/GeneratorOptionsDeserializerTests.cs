using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using DatabaseBenchmark.Model;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace DatabaseBenchmark.Tests.Generators
{
    public class GeneratorOptionsDeserializerTests
    {
        public static IEnumerable<object[]> SampleData =>
        [
            ["{\"Type\":\"Address\", \"Kind\":\"FullAddress\"}",
               new AddressGeneratorOptions { Kind = AddressGeneratorOptions.GeneratorKind.FullAddress }],
            ["{\"Type\":\"Boolean\", \"Weight\":0.6}",
               new BooleanGeneratorOptions { Weight = 0.6f }],
            ["{\"Type\":\"Company\", \"Kind\":\"CompanySuffix\"}",
               new CompanyGeneratorOptions { Kind = CompanyGeneratorOptions.GeneratorKind.CompanySuffix }],
            ["{\"Type\":\"DateTime\", \"MinValue\":\"2012-01-02T12:12:12\", \"MaxValue\":\"2020-03-04T13:14:15\", \"Increasing\":true, \"Delta\":\"03:00:00\", \"RandomizeDelta\":true}",
               new DateTimeGeneratorOptions { MinValue = new DateTime(2012, 1, 2, 12, 12, 12), MaxValue = new DateTime(2020, 3, 4, 13, 14, 15), Increasing = true, Delta = TimeSpan.FromHours(3), RandomizeDelta = true }],
            ["{\"Type\":\"Finance\", \"Kind\":\"CreditCardNumber\"}",
               new FinanceGeneratorOptions { Kind = FinanceGeneratorOptions.GeneratorKind.CreditCardNumber }],
            ["{\"Type\":\"Float\", \"MinValue\":100, \"MaxValue\":1000, \"Increasing\":true, \"Delta\":3, \"RandomizeDelta\":true}",
               new FloatGeneratorOptions { MinValue = 100, MaxValue = 1000, Increasing = true, Delta = 3, RandomizeDelta = true }],
            ["{\"Type\":\"ForeignColumn\", \"TableName\":\"table\", \"ColumnName\":\"column\", \"ColumnType\":\"String\"}",
               new ForeignColumnGeneratorOptions { TableName = "table", ColumnName = "column", ColumnType = ColumnType.String }],
            ["{\"Type\":\"Guid\"}",
               new GuidGeneratorOptions() ],
            ["{\"Type\":\"Integer\", \"MinValue\":100, \"MaxValue\":1000, \"Increasing\":true, \"Delta\":3, \"RandomizeDelta\":true}",
               new IntegerGeneratorOptions { MinValue = 100, MaxValue = 1000, Increasing = true, Delta = 3, RandomizeDelta = true }],
            ["{\"Type\":\"Internet\", \"Kind\":\"Ip\"}",
               new InternetGeneratorOptions { Kind = InternetGeneratorOptions.GeneratorKind.Ip }],
            ["{\"Type\":\"ListItem\", \"Items\":[\"one\", \"two\", \"three\"]}",
               new ListItemGeneratorOptions { Items = ["one", "two", "three"] }],
            ["{\"Type\":\"Name\", \"Kind\":\"FullName\"}",
               new NameGeneratorOptions { Kind = NameGeneratorOptions.GeneratorKind.FullName }],
            ["{\"Type\":\"Phone\", \"Kind\":\"PhoneNumber\"}",
               new PhoneGeneratorOptions { Kind = PhoneGeneratorOptions.GeneratorKind.PhoneNumber }],
            ["{\"Type\":\"String\", \"MinLength\":3, \"MaxLength\": 8, \"AllowedCharacters\":\"ABCXYZ\"}", 
               new StringGeneratorOptions { MinLength = 3, MaxLength = 8, AllowedCharacters = "ABCXYZ" }],
            ["{\"Type\":\"Text\", \"Kind\":\"Paragraph\"}",
               new TextGeneratorOptions { Kind = TextGeneratorOptions.GeneratorKind.Paragraph }],
            ["{\"Type\":\"Vehicle\", \"Kind\":\"Vin\"}",
                new VehicleGeneratorOptions { Kind = VehicleGeneratorOptions.GeneratorKind.Vin }]
        ];

        [Theory]
        [MemberData(nameof(SampleData))]
        public void DeserializeOptions(string sampleJson, IGeneratorOptions sampleOptions)
        {
            using var jsonDocument = JsonDocument.Parse(sampleJson);
            var options = GeneratorOptionsDeserializer.Deserialize(jsonDocument.RootElement);

            var serializedOptions = JsonSerializer.Serialize(options, options.GetType());
            var serializedSampleOptions = JsonSerializer.Serialize(sampleOptions, sampleOptions.GetType());

            Assert.Equal(sampleOptions.GetType(), options.GetType());
            Assert.Equal(serializedSampleOptions, serializedOptions);
        }
    }
}
