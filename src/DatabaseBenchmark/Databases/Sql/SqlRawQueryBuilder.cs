using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
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
                    var aliases = rawCollection.Select(v => BuildParameter(parameter, v)).ToArray();
                    parameterString = string.Join(", ", aliases);
                }
                else
                {
                    parameterString = BuildParameter(parameter, rawValue);
                }

                queryText = queryText.Replace($"${{{parameter.Name}}}", parameterString);
            }

            return queryText;
        }

        private string BuildParameter(RawQueryParameter parameter, object value) =>
            parameter.Inline
                ? InlineParameterFormatter.Format(parameter.InlineFormat, value)
                : _parametersBuilder.Append(value, parameter.Type);
    }
}
