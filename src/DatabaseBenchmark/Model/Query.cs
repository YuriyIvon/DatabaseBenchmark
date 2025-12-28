using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Model
{
    public class Query
    {
        public bool Distinct { get; set; } = false;

        public string[] Columns { get; set; }

        public QueryRanking Ranking { get; set; }

        [JsonConverter(typeof(JsonQueryConditionConverter))]
        public IQueryCondition Condition { get; set; }

        public QueryAggregate Aggregate { get; set; }

        public QuerySort[] Sort { get; set; }

        public int Skip { get; set; }

        public int Take { get; set; }
    }
}
