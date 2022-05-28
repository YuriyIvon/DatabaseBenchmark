using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using System.Reflection;

namespace DatabaseBenchmark.Commands
{
    public abstract class OptionsProvider : IOptionsProvider
    {
        protected IDictionary<string, string> Options { get; set; }

        public T GetOptions<T>() where T : new()
        {
            if (Options == null)
            {
                ParseOptions();
            }

            var prefix = typeof(T).GetCustomAttribute<OptionPrefixAttribute>()?.Prefix;

            var commandParameters = typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                .Select(p => new OptionDescriptor(p, p.GetCustomAttribute<OptionAttribute>()))
                .Where(p => p.PropertyAttribute != null)
                .ToDictionary(p => FormatProperty(prefix, p.Property.Name));

            var missingParameters = commandParameters
                .Where(p => p.Value.PropertyAttribute.IsRequired)
                .Select(p => p.Key)
                .Except(Options.Keys)
                .ToArray();

            if (missingParameters.Any())
            {
                throw new InputArgumentException($"The following required parameters are missing: {string.Join(", ", missingParameters)}");
            }

            var options = new T();

            foreach (var ip in Options)
            {
                if (commandParameters.TryGetValue(ip.Key, out var cp))
                {
                    //TODO: error handling for type conversion
                    Type valueType = Nullable.GetUnderlyingType(cp.Property.PropertyType) ?? cp.Property.PropertyType;
                    object value = (ip.Value == null) ? null : Convert.ChangeType(ip.Value, valueType);
                    cp.Property.SetValue(options, value);
                }
            }

            return options;
        }

        protected abstract void ParseOptions();

        protected static string FormatProperty(string prefix, string propertyName) =>
            prefix != null ? string.Join(".", prefix, propertyName) : propertyName;
    }
}
