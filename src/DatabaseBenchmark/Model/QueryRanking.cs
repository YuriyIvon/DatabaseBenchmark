using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Model
{
    public class QueryRanking
    {
        public RankingQueryFusionStrategy FusionStrategy { get; set; } = RankingQueryFusionStrategy.ReciprocalRankFusion;

        [JsonConverter(typeof(JsonRankingQueryArrayConverter))]
        public IRankingQuery[] Queries { get; set; }
    }
}
