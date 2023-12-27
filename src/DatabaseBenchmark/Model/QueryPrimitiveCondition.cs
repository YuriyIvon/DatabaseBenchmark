using DatabaseBenchmark.Common;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Model
{
    public class QueryPrimitiveCondition : IQueryCondition
    {
        public string ColumnName { get; set; }

        public QueryPrimitiveOperator Operator { get; set; }

        [JsonConverter(typeof(JsonObjectConverter))]
        public object Value { get; set; }

        public bool RandomizeValue { get; set; } = false;

        public bool RandomizeInclusion { get; set; } = false;

        public ValueRandomizationRule ValueRandomizationRule { get; set; } = new ValueRandomizationRule();
    }
}
