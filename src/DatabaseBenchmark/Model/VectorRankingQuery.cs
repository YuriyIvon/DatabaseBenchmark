using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Model
{
    public class VectorRankingQuery : IRankingQuery
    {
        public RankingQueryType Type => RankingQueryType.Vector;

        public string ColumnName { get; set; }

        public float[] Vector { get; set; }

        public VectorSimilarityMetric Metric { get; set; } = VectorSimilarityMetric.Cosine;

        [JsonConverter(typeof(JsonQueryConditionConverter))]
        public IQueryCondition Condition { get; set; }

        public bool Exact { get; set; } = false;

        public int Limit { get; set; } = 10;

        public int? Candidates { get; set; }

        public float? Weight { get; set; }

        public bool RandomizeVector { get; set; } = false;

        public ValueRandomizationRule VectorRandomizationRule { get; set; } = new ValueRandomizationRule();
    }
}
