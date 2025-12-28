using System.Text.Json;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Model
{
    public class JsonRankingQueryArrayConverter : JsonConverter<IRankingQuery[]>
    {
        private readonly JsonRankingQueryConverter _itemConverter = new();

        public override IRankingQuery[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                var items = new List<IRankingQuery>();

                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    items.Add(_itemConverter.Read(ref reader, typeof(IRankingQuery), options));
                }

                return items.ToArray();
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, IRankingQuery[] value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }
}
