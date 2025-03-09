using DatabaseBenchmark.Model;
using MongoDB.Bson;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public static class MongoDbValueAdapter
    {
        public static BsonValue CreateBsonValue(ColumnType columnType, object value) =>
            value switch
            {
                Guid guidValue when columnType == ColumnType.Guid => new BsonBinaryData(guidValue, GuidRepresentation.Standard),
                string stringValue when columnType == ColumnType.Guid => new BsonBinaryData(new Guid(stringValue), GuidRepresentation.Standard),
                _ => BsonValue.Create(value)
            };
    }
}
