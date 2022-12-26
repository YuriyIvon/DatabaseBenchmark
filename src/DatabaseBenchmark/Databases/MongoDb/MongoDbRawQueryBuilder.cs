using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.MongoDb.Interfaces;
using DatabaseBenchmark.Model;
using MongoDB.Bson;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public class MongoDbRawQueryBuilder : IMongoDbQueryBuilder
    {
        private readonly RawQuery _query;
        private readonly IRandomValueProvider _randomValueProvider;

        public MongoDbRawQueryBuilder(
            RawQuery query,
            IRandomValueProvider randomValueProvider)
        {
            _query = query;
            _randomValueProvider = randomValueProvider;
        }

        public IEnumerable<BsonDocument> Build()
        {
            var queryText = _query.Text;

            if (_query.Parameters != null)
            {
                queryText = ApplyParameters(queryText);
            }

            var bsonQuery = MongoDB.Bson.Serialization
                   .BsonSerializer.Deserialize<IEnumerable<BsonDocument>>(queryText);

            return bsonQuery;
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
    }
}
