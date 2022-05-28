using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Model
{
    public class QueryGroupCondition : IQueryCondition
    {
        public QueryGroupOperator Operator { get; set; }

        [JsonConverter(typeof(JsonQueryConditionArrayConverter))]
        public IQueryCondition[] Conditions { get; set; }

        public bool RandomizeInclusion { get; set; } = false;
    }
}
