using Amazon.DynamoDBv2.Model;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.DynamoDb
{
    public static class DynamoDbAttributeValueUtils
    {
        public static object FromAttributeValue(AttributeValue attributeValue) =>
            attributeValue switch
            {
                { NULL: true } => null,
                { IsBOOLSet: true } => attributeValue.BOOL,
                { N: var n } when n != null => n,
                _ => attributeValue.S,
            };

        public static AttributeValue ToAttributeValue(ColumnType type, object value)
        {
            if (value == null)
            {
                return new AttributeValue { NULL = true };
            }
            else
            {
                return type switch
                {
                    ColumnType.Boolean => new AttributeValue { IsBOOLSet = true, BOOL = (bool)value },
                    ColumnType.Double => new AttributeValue { N = value.ToString() },
                    ColumnType.Integer => new AttributeValue { N = value.ToString() },
                    ColumnType.Long => new AttributeValue { N = value.ToString() },
                    ColumnType.Text => new AttributeValue(value.ToString()),
                    ColumnType.String => new AttributeValue(value.ToString()),
                    ColumnType.DateTime => new AttributeValue(((DateTime)value).ToString("s")),
                    ColumnType.Guid => new AttributeValue(value.ToString()),
                    ColumnType.Json => new AttributeValue(value.ToString()),
                    _ => throw new InputArgumentException($"Unknown column type \"{type}\"")
                };
            }
        }
    }
}
