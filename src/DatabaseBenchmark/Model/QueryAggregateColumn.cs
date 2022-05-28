namespace DatabaseBenchmark.Model
{
    public class QueryAggregateColumn
    {
        public QueryAggregateFunction Function { get; set; }

        public string ResultColumnName { get; set; }

        public string SourceColumnName { get; set; }
    }
}
