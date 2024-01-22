using System.Text.Json;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Model
{
    public class JsonQueryConditionArrayConverter : JsonConverter<IQueryCondition[]>
    {
        private readonly JsonQueryConditionConverter _itemConverter = new();

        public override IQueryCondition[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                var items = new List<IQueryCondition>();

                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    items.Add(_itemConverter.Read(ref reader, typeof(object), options));
                }

                return items.ToArray();
            }
 
            return null;
        }

        public override void Write(Utf8JsonWriter writer, IQueryCondition[] value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }
}
