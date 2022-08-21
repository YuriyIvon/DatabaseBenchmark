namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlParametersBuilder
    {
        private readonly string _prefix;

        private int _counter = 0;

        public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();

        public SqlParametersBuilder(string prefix = "@")
        {
            _prefix = prefix;
        }

        public string Append(object value)
        {
            string name = $"{_prefix}p{_counter}";
            _counter++;

            Values.Add(name, PrepareValue(value));

            return name;
        }

        public void Reset()
        {
            _counter = 0;
            Values.Clear();
        }

        protected virtual object PrepareValue(object value) => value;
    }
}
