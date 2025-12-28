namespace DatabaseBenchmark.Model
{
    public interface IRankingQuery
    {
        RankingQueryType Type { get; }

        float? Weight { get; set; }
    }
}
