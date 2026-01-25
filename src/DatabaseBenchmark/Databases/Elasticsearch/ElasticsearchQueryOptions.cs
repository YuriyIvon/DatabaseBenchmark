using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    [OptionPrefix("Elasticsearch")]
    public class ElasticsearchQueryOptions
    {
        [Option("Use retriever feature of Elasticsearch to implement vector search queries")]
        public bool UseRetriever { get; set; } = true;
    }
}
