using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Elasticsearch.Interfaces;
using DatabaseBenchmark.Model;
using Elasticsearch.Net;
using Nest;
using System.Reflection;
using System.Text;
using System.Text.Json;
using RawQuery = DatabaseBenchmark.Model.RawQuery;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public class ElasticsearchRawQueryBuilder : IElasticsearchQueryBuilder
    {
        private readonly RawQuery _query;
        private readonly IElasticsearchSerializer _serializer;
        private readonly IRandomValueProvider _randomValueProvider;

        public ElasticsearchRawQueryBuilder(
            RawQuery query,
            IElasticsearchSerializer serializer,
            IRandomValueProvider randomValueProvider)
        {
            _query = query;
            _serializer = serializer;
            _randomValueProvider = randomValueProvider;
        }

        public SearchRequest Build()
        {
            _randomValueProvider?.Next();

            var queryText = _query.Text;

            if (_query.Parameters != null)
            {
                queryText = ApplyParameters(queryText);
            }

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(queryText));
            var basicSearchRequest = _serializer.Deserialize<SearchRequest>(stream);

            var searchRequest = new SearchRequest(_query.TableName);
            //Since it is not possible to set index on a deserialized search request,
            //need to create a blank request with correct index and then copy the rest of properties
            CopyPublicProperties(basicSearchRequest, searchRequest);

            return searchRequest;
        }

        private string ApplyParameters(string queryText)
        {
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

        private static void CopyPublicProperties<T>(T source, T destination)
        {
            foreach(var property in typeof(T).GetProperties(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                var value = property.GetValue(source, null);
                if (value != null)
                {
                    property.SetValue(destination, value);
                }
            }
        }

        private static string FormatParameter(RawQueryParameter parameter, object value) =>
            parameter.Inline 
                ? InlineParameterFormatter.Format(parameter.InlineFormat, value)
                : JsonSerializer.Serialize(value);
    }
}
