namespace DatabaseBenchmark.Common
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class OptionPrefixAttribute : Attribute
    {
        public string Prefix { get; set; }

        public OptionPrefixAttribute(string prefix)
        {
            Prefix = prefix;
        }
    }
}
