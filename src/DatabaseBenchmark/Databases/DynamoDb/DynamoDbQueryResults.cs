using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DatabaseBenchmark.Databases.Common.Interfaces;

namespace DatabaseBenchmark.Databases.DynamoDb
{
    public class DynamoDbQueryResults : IQueryResults
    {
        private readonly AmazonDynamoDBClient _client;
        private readonly ExecuteStatementRequest _request;

        private ExecuteStatementResponse _response;
        private int _responseItemIndex = 0;

        public double ConsumedCapacity { get; private set; }

        public IEnumerable<string> ColumnNames => _response.Items[_responseItemIndex].Keys;

        public DynamoDbQueryResults(
            AmazonDynamoDBClient client,
            ExecuteStatementRequest request)
        {
            _client = client;
            _request = request;
        }

        public object GetValue(string columnName) =>
            DynamoDbAttributeValueUtils.FromAttributeValue(_response.Items[_responseItemIndex][columnName]);

        public bool Read()
        {
            if (_response != null && _responseItemIndex < _response.Items.Count)
            {
                _responseItemIndex++;
            }

            if (_response == null ||
                (_responseItemIndex >= _response.Items.Count && _response.NextToken != null))
            {
                _request.NextToken = _response?.NextToken;
                _response = _client.ExecuteStatementAsync(_request).Result;

                ConsumedCapacity += _response.ConsumedCapacity.CapacityUnits.GetValueOrDefault();

                return _response.Items.Any();
            }

            return _responseItemIndex < _response.Items.Count;
        }
    }
}
