namespace DatabaseBenchmark.Databases.AzureSearch
{
    public class AzureSearchRawQuery
    {
        public string[] Select { get; set; }

        public string Filter { get; set; }

        public string[] OrderBy { get; set; }

        public int? Skip { get; set; }

        public int? Size { get; set; }
    }
}
