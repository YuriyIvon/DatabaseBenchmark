namespace DatabaseBenchmark.Databases.CosmosDb
{
    public class CosmosDbQueryResult<T>
    {
        public List<T> Items { get; }

        public double TotalRequestCharge { get; }

        public CosmosDbQueryResult(List<T> items, double totalRequestCharge)
        {
            Items = items;
            TotalRequestCharge = totalRequestCharge;
        }
    }
}
