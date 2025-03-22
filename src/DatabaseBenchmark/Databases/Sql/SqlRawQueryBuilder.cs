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
            _randomValueProvider?.Next();

            var queryText = _query.Text;

            if (_query.Parameters != null)
            {
                queryText = ApplyParameters(queryText);
            }

            return queryText;
        }

        protected virtual string BuildParameter(RawQueryParameter parameter, object value) =>
            parameter.Inline
                ? InlineParameterFormatter.Format(parameter.InlineFormat, value)
                : _parametersBuilder.Append(value, parameter.Type, parameter.Array);

        protected virtual string BuildParameterList(RawQueryParameter parameter, IEnumerable<object> collectionValue)
        {
            var aliases = collectionValue.Select(v => BuildParameter(parameter, v)).ToArray();
            return string.Join(", ", aliases);
        }

        private string ApplyParameters(string queryText)
        {
            foreach (var parameter in _query.Parameters)
            {
                var rawValue = !parameter.RandomizeValue
                    ? parameter.Value
                    : parameter.Collection
                        ? _randomValueProvider.GetValueCollection(null, parameter, parameter.ValueRandomizationRule)
                        : _randomValueProvider.GetValue(null, parameter, parameter.ValueRandomizationRule);

                var parameterString = rawValue switch
                {
                    IEnumerable<object> collectionValue when !parameter.Array => BuildParameterList(parameter, collectionValue),
                    _ => BuildParameter(parameter, rawValue)
                };

                queryText = queryText.Replace($"${{{parameter.Name}}}", parameterString);
            }

            return queryText;
        }
    }
}
