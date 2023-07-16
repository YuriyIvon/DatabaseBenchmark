namespace DatabaseBenchmark.Common
{
    public static class ConnectionStringParser
    {
        public static IDictionary<string, string> Parse(string connectionString, params string[] requiredKeys)
        {
            var keyValues = connectionString.Split(';')
                .Select(part => part.Split('=', 2))
                .Where(part => part.Length == 2)
                .ToDictionary(sp => sp[0], sp => sp[1]);

            foreach (var key in requiredKeys)
            {
                if (!keyValues.ContainsKey(key))
                {
                    throw new InputArgumentException($"The required key \"{key}\" was not found in the connection string");
                }
            }

            return keyValues;
        }
    }
}
