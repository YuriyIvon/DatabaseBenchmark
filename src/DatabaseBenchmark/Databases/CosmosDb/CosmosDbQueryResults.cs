using DatabaseBenchmark.Databases.Common.Interfaces;
using Microsoft.Azure.Cosmos;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public sealed class CosmosDbQueryResults : IQueryResults, IDisposable
    {
        private readonly FeedIterator<Dictionary<string, object>> _iterator;

        private Dictionary<string, object>[] _responseItems;
        private int _responseItemIndex = 0;

        public IEnumerable<string> ColumnNames => _responseItems[_responseItemIndex].Keys;

        public double RequestCharge { get; private set; }

        public CosmosDbQueryResults(FeedIterator<Dictionary<string, object>> iterator)
        {
            _iterator = iterator;
        }

        public object GetValue(string columnName) => _responseItems[_responseItemIndex][columnName];

        public bool Read()
        {
            if (_responseItems != null && _responseItemIndex < _responseItems.Length)
            {
                _responseItemIndex++;
            }

            if (_responseItems == null ||
                (_responseItemIndex >= _responseItems.Length && _iterator.HasMoreResults))
            {
                var response = _iterator.ReadNextAsync().Result;
                _responseItems = response.ToArray();
                _responseItemIndex = 0;
                RequestCharge += response.RequestCharge;

                return _responseItems.Any();
            }

            return _responseItemIndex < _responseItems.Length;
        }

        public void Dispose() => _iterator?.Dispose();
    }
}
