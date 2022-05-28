namespace DatabaseBenchmark.Databases.Model
{
    public class ImportResult
    {
        public long Count { get; set; }

        public long Duration { get; set; }

        public long? TotalStorageBytes { get; set; }
    }
}
