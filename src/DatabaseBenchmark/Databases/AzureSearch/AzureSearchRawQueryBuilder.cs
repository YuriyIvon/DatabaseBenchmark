using Azure.Search.Documents;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.AzureSearch.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Model;
using System.Text.Json;
using RawQuery = DatabaseBenchmark.Model.RawQuery;

namespace DatabaseBenchmark.Databases.AzureSearch
{
    public class AzureSearchRawQueryBuilder : IAzureSearchQueryBuilder
    {
        private readonly RawQuery _query;
        private readonly IRandomValueProvider _randomValueProvider;
        private readonly IValueFormatter _valueFormatter = new AzureSearchValueFormatter();

        public AzureSearchRawQueryBuilder(
            RawQuery query,
            IRandomValueProvider randomValueProvider)
        {
            _query = query;
            _randomValueProvider = randomValueProvider;
        }

        public SearchOptions Build()
        {
            _randomValueProvider?.Next();

            var inputOptions = JsonSerializer.Deserialize<AzureSearchRawQuery>(_query.Text);

            var options = new SearchOptions
            {
                Filter = _query.Parameters != null ? ApplyParameters(inputOptions.Filter) : inputOptions.Filter,
                Skip = inputOptions.Skip,
                Size = inputOptions.Size
            };

            inputOptions.Select?.ToList().ForEach(options.Select.Add);
            inputOptions.OrderBy?.ToList().ForEach(options.OrderBy.Add);

            return options;
        }

        private string ApplyParameters(string queryText)
        {
            if (string.IsNullOrEmpty(queryText))
            {
                return null;
            }

            foreach (var parameter in _query.Parameters)
            {
                string parameterString;

                var rawValue = !parameter.RandomizeValue
                    ? parameter.Value
                    : parameter.Collection
                        ? _randomValueProvider.GetValueCollection(null, parameter, parameter.ValueRandomizationRule)
                        : _randomValueProvider.GetValue(null, parameter, parameter.ValueRandomizationRule);

                parameterString = FormatParameter(parameter, rawValue);

                queryText = queryText.Replace($"${{{parameter.Name}}}", parameterString);
            }

            return queryText;
        }

        private string FormatParameter(RawQueryParameter parameter, object value) =>
            parameter.Inline 
                ? InlineParameterFormatter.Format(parameter.InlineFormat, value)
                : _valueFormatter.Format(value);
    }
}