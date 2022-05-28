namespace DatabaseBenchmark.Model
{
    public class RawQuery
    {
        public string Text { get; set; }

        public string TableName { get; set; }

        public RawQueryParameter[] Parameters { get; set; }
    }
}
