using DatabaseBenchmark.DataSources.Interfaces;
using MongoDB.Bson;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public sealed class MongoDbDataSourceWrapper : IDataSource
    {
        private readonly IDataSource _dataSource;

        public MongoDbDataSourceWrapper(IDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public object GetValue(string name) => WrapGuidValue(_dataSource.GetValue(name));

        private object WrapGuidValue(object value) =>
            value switch
            {
                IEnumerable<object> arrayValue => arrayValue.Select(WrapGuidValue).ToArray(),
                Guid guidValue => new BsonBinaryData(guidValue, GuidRepresentation.Standard),
                _ => value
            };

        public bool Read() => _dataSource.Read();

        public void Dispose() => _dataSource.Dispose();
    }
}
