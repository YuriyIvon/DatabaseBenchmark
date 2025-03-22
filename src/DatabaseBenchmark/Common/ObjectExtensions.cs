namespace DatabaseBenchmark.Common
{
    public static class ObjectExtensions
    {
        public static bool IsNumber(this object value) => value is byte or short or ushort or int or uint or long or ulong or double or float or decimal;
    }
}
