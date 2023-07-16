using Amazon.DynamoDBv2.Model;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.DynamoDb
{
    public static class DynamoDbAttributeValueUtils
    {
        public static object FromAttributeValue(AttributeValue attributeValue)
        {
            if (attributeValue.NULL)
            {
                return null;
            }
            else if (attributeValue.IsBOOLSet)
            {
                return attributeValue.BOOL;
            }
            else if (attributeValue.N != null)
            {
                return attributeValue.N;
            }
            else
            {
                return attributeValue.S;
            }
        }

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
                    ColumnType.Boolean => new AttributeValue { BOOL = (bool)value, IsBOOLSet = true },
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
