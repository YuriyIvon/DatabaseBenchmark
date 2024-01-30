using DatabaseBenchmark.Common;
using System.Reflection;

namespace DatabaseBenchmark.Commands
{
    public class OptionDescriptor
    {
        public PropertyInfo Property { get; }

        public OptionAttribute PropertyAttribute { get; }

        public OptionDescriptor(PropertyInfo property, OptionAttribute parameterAttribute)
        {
            Property = property;
            PropertyAttribute = parameterAttribute;
        }
    }
}
