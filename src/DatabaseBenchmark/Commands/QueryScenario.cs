using System.Text.Json;

namespace DatabaseBenchmark.Commands
{
    public class QueryScenario
    {
        public string Name { get; set; }

        public JsonElement[] Items { get; set; }
    }
}
