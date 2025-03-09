using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.MongoDb.Interfaces;
using DatabaseBenchmark.Model;
using MongoDB.Bson;
using System.Text.Json;

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
            _randomValueProvider?.Next();

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
                        ? _randomValueProvider.GetValueCollection(null, parameter, parameter.ValueRandomizationRule)
                        : _randomValueProvider.GetValue(null, parameter, parameter.ValueRandomizationRule);

                if (rawValue is IEnumerable<object> rawCollection)
                {
                    var aliases = rawCollection.Select(v => FormatParameter(parameter, v)).ToArray();
                    parameterString = string.Join(", ", aliases);
                }
                else
                {
                    parameterString = FormatParameter(parameter, rawValue);
                }

                queryText = queryText.Replace($"${{{parameter.Name}}}", parameterString);
            }

            return queryText;
        }

        private static string FormatParameter(RawQueryParameter parameter, object value) =>
            parameter.Inline
                ? InlineParameterFormatter.Format(parameter.InlineFormat, value)
                : MongoDbValueAdapter.CreateBsonValue(parameter.Type, value).ToJson();
    }
}
