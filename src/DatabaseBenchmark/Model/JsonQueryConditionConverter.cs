using System.Text.Json;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Model
{
    public class JsonQueryConditionConverter : JsonConverter<IQueryCondition>
    {
        public override IQueryCondition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var predicateJson = JsonDocument.ParseValue(ref reader);
                var operatorString = predicateJson.RootElement.GetProperty("Operator").GetString();
                if (Enum.IsDefined(typeof(QueryPrimitiveOperator), operatorString))
                {
                    return (QueryPrimitiveCondition)JsonSerializer.Deserialize(
                        predicateJson.RootElement.ToString(),
                        typeof(QueryPrimitiveCondition),
                        options);
                }
                else if (Enum.IsDefined(typeof(QueryGroupOperator), operatorString))
                {
                    return (QueryGroupCondition)JsonSerializer.Deserialize(
                        predicateJson.RootElement.ToString(),
                        typeof(QueryGroupCondition),
                        options);
                }
                else
                {
                    throw new JsonException("Can't deserialize a predicate");
                }
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, IQueryCondition value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

}
