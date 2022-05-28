namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlParametersBuilder
    {
        private int _counter = 0;

        public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();

        public string Append(object value)
        {
            string name = $"@p{_counter}";
            _counter++;

            Values.Add(name, value);

            return name;
        }

        public void Reset()
        {
            _counter = 0;
            Values.Clear();
        }
    }
}
