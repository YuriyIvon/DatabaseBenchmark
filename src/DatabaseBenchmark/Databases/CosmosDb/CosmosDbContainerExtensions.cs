using Microsoft.Azure.Cosmos;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public static class CosmosDbContainerExtensions
    {
        public static CosmosDbQueryResult<T> Query<T>(this Container container, PartitionKey partitionKey, string query) =>
            Query<T>(container, partitionKey, new QueryDefinition(query));

        public static CosmosDbQueryResult<T> Query<T>(this Container container, PartitionKey partitionKey, QueryDefinition queryDefinition)
        {
            using var iterator = container.GetItemQueryIterator<T>(
                queryDefinition,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = partitionKey
                });

            var items = new List<T>();
            double totalRequestCharge = 0;

            while (iterator.HasMoreResults)
            {
                var result = iterator.ReadNextAsync().Result;

                items.AddRange(result);
                totalRequestCharge += result.RequestCharge;
            }

            return new CosmosDbQueryResult<T>(items, totalRequestCharge);
        }

        public static int Count(this Container container)
        {
            int result = 0;

            var iterator = container.GetItemQueryIterator<int>("SELECT VALUE COUNT(1) FROM c");
            while (iterator.HasMoreResults)
            {
                foreach (var item in iterator.ReadNextAsync().Result)
                {
                    result += item;
                }
            }

            return result;
        }
    }
}
