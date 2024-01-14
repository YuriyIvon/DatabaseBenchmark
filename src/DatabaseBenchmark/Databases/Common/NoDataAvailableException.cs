namespace DatabaseBenchmark.Databases.Common
{
    public class NoDataAvailableException : Exception
    {
        public NoDataAvailableException() : base("No data avaiable")
        {
        }
    }
}
