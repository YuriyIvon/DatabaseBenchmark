using System.Text.Json;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Common
{
    public class JsonObjectArrayConverter : JsonConverter<object[]>
    {
        private readonly JsonObjectConverter _itemConverter = new();

        public override object[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                var items = new List<object>();

                while(reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    items.Add(_itemConverter.Read(ref reader, typeof(object), options));
                }

                return items.ToArray();
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, object[] value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
