namespace DatabaseBenchmark.Common
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class OptionAttribute : Attribute
    {
        public OptionAttribute(string description, bool isRequired = false)
        {
            Description = description;
            IsRequired = isRequired;
        }

        public string Description { get; set; }

        public bool IsRequired { get; set; } = false;
    }
}
