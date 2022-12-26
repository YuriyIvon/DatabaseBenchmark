using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Elasticsearch.Interfaces;
using Elasticsearch.Net;
using Nest;
using System.Reflection;
using System.Text;
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
                        ? _randomValueProvider.GetRandomValueCollection(null, parameter.Name, parameter.ValueRandomizationRule)
                        : _randomValueProvider.GetRandomValue(null, parameter.Name, parameter.ValueRandomizationRule);

                if (rawValue is IEnumerable<object> rawCollection)
                {
                    var aliases = rawCollection.Select(v => FormatValue(v)).ToArray();
                    parameterString = string.Join(", ", aliases);
                }
                else
                {
                    parameterString = FormatValue(rawValue);
                }

                queryText = queryText.Replace($"${{{parameter.Name}}}", parameterString);
            }

            return queryText;
        }

        private static string FormatValue(object value)
        {
            if (value != null)
            {
                var stringValue = value.ToString();

                return (value is bool || value is int || value is long || value is double)
                    ? stringValue : $"\"{stringValue.Replace("\"", "\\\"")}\"";
            }

            return "null";
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
    }
}
