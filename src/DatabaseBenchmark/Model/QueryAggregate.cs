namespace DatabaseBenchmark.Model
{
    public class QueryAggregate
    {
        public string[] GroupColumnNames { get; set; }

        public QueryAggregateColumn[] ResultColumns { get; set; }
    }
}
