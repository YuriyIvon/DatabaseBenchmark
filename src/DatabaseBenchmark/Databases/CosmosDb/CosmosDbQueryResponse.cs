namespace DatabaseBenchmark.Databases.CosmosDb
{
    public class CosmosDbQueryResponse<T>
    {
        public List<T> Items { get; }

        public double TotalRequestCharge { get; }

        public CosmosDbQueryResponse(List<T> items, double totalRequestCharge)
        {
            Items = items;
            TotalRequestCharge = totalRequestCharge;
        }
    }
}
