using System.Text.Json;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Common
{
    public static class JsonUtils
    {
        public static T DeserializeFile<T>(string filePath, params JsonConverter[] converters)
        {
            var json = File.ReadAllText(filePath);

            var options = new JsonSerializerOptions();
            converters?.ToList().ForEach(options.Converters.Add);

            return JsonSerializer.Deserialize<T>(json, options);
        }
    }
}
