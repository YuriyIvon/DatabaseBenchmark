using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Model
{
    public class ValueRandomizationRule
    {
        public bool UseExistingValues { get; set; } = true;

        [JsonConverter(typeof(JsonObjectArrayConverter))]
        public object[] ExistingValuesOverride { get; set; } = null;

        public string ExistingValuesSourceTableName { get; set; } = null;

        public string ExistingValuesSourceColumnName { get; set; } = null;

        public int MinCollectionLength { get; set; } = 1;

        public int MaxCollectionLength { get; set; } = 10;

        public int MinNumericValue { get; set; } = 0;

        public int MaxNumericValue { get; set; } = 100;

        public DateTime MinDateTimeValue { get; set; } = DateTime.Now.AddYears(-10);

        public DateTime MaxDateTimeValue { get; set; } = DateTime.Now;

        public int MinStringValueLength { get; set; } = 1;

        public int MaxStringValueLength { get; set; } = 10;

        public string AllowedCharacters { get; set; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
    }
}
