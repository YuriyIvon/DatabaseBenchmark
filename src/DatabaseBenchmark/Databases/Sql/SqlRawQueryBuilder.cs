using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlRawQueryBuilder : ISqlQueryBuilder
    {
        private readonly RawQuery _query;
        private readonly ISqlParametersBuilder _parametersBuilder;
        private readonly IRandomValueProvider _randomValueProvider;

        public SqlRawQueryBuilder(
            RawQuery query,
            ISqlParametersBuilder parametersBuilder,
            IRandomValueProvider randomValueProvider)
        {
            _query = query;
            _parametersBuilder = parametersBuilder;
            _randomValueProvider = randomValueProvider;
        }

        public string Build()
        {
            _parametersBuilder.Reset();

            var queryText = _query.Text;

            if (_query.Parameters != null)
            {
                queryText = ApplyParameters(queryText);
            }

            return queryText;
        }

        private string ApplyParameters(string queryText)
        {
            foreach (var parameter in _query.Parameters)
            {
                string parameterString;

                var rawValue = !parameter.RandomizeValue
                    ? parameter.Value
                    : parameter.Collection
                        ? _randomValueProvider.GetRandomValueCollection(null, parameter.Name, parameter.ValueRandomizationRule)
                        : _randomValueProvider.GetRandomValue(null, parameter.Name, parameter.ValueRandomizationRule);

                if (rawValue is IEnumerable<object> rawCollection)
                {
                    var aliases = rawCollection.Select(v => _parametersBuilder.Append(v, parameter.Type)).ToArray();
                    parameterString = string.Join(", ", aliases);
                }
                else
                {
                    parameterString = _parametersBuilder.Append(rawValue, parameter.Type);
                }

                queryText = queryText.Replace($"${{{parameter.Name}}}", parameterString);
            }

            return queryText;
        }
    }
}
