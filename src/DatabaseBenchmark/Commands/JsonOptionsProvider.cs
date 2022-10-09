using DatabaseBenchmark.Common;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DatabaseBenchmark.Commands
{
    public class JsonOptionsProvider : OptionsProvider
    {
        private readonly string _sourceJson;
        private readonly string _parametersJson;

        public JsonOptionsProvider(string sourceJson, string parametersJson = null)
        {
            _sourceJson = sourceJson;
            _parametersJson = parametersJson;
        }

        protected override void ParseOptions()
        {
            Options = ParseDictionary(_sourceJson);

            if (_parametersJson != null)
            {
                var parameters = ParseDictionary(_parametersJson);

                //Add all properties from the parameters file that don't exists in the input file
                foreach (var parameter in parameters)
                {
                    if (!Options.ContainsKey(parameter.Key))
                    {
                        Options.Add(parameter.Key, parameter.Value.ToString());
                    }
                }

                //Replace placeholders in property values with the corresponding parameter values
                foreach (var propertyName in Options.Keys)
                {
                    Options[propertyName] = ExpandParameters(Options[propertyName], parameters);
                }
            }
        }

        private static string ExpandParameters(string text, IDictionary<string, string> parameters) =>
            Regex.Replace(text, @"\$\{(\w+?)\}", match => parameters[match.Groups[1].Value]);

        private static IDictionary<string, string> ParseDictionary(string json)
        {
            var dictionary = new Dictionary<string, string>();
            var jsonDocument = JsonDocument.Parse(json);

            foreach (var property in jsonDocument.RootElement.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    dictionary.Add(property.Name, string.Join(",", property.Value.EnumerateArray()));
                }
                else if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    throw new InputArgumentException("Object properties are not supported");
                }
                else if (property.Value.ValueKind != JsonValueKind.Null)
                {
                    dictionary.Add(property.Name, property.Value.ToString());
                }
            }

            return dictionary;
        }
    }
}
