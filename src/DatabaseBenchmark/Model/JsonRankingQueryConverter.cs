using DatabaseBenchmark.Common;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Model
{
    public class JsonRankingQueryConverter : JsonConverter<IRankingQuery>
    {
        public override IRankingQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                using var rankingQueryJson = JsonDocument.ParseValue(ref reader);

                if (!rankingQueryJson.RootElement.TryGetProperty(nameof(IRankingQuery.Type), out var typeElement))
                {
                    throw new InputArgumentException($"Property \"{nameof(IRankingQuery.Type)}\" not found in the ranking query");
                }

                var type = (RankingQueryType)Enum.Parse(typeof(RankingQueryType), typeElement.GetString());

                return type switch
                {
                    RankingQueryType.Vector => rankingQueryJson.Deserialize<VectorRankingQuery>(options),
                    _ => throw new InputArgumentException($"Unknown ranking query type \"{type}\"")
                };
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, IRankingQuery value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }
}
