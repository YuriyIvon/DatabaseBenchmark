using System.Text.Json;

namespace DatabaseBenchmark.Utils
{
    public static class JsonUtils
    {
        public static T DeserializeFile<T>(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
