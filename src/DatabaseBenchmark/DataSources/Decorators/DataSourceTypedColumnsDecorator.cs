using DatabaseBenchmark.Common;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DatabaseBenchmark.DataSources.Decorators
{
    public sealed class DataSourceTypedColumnsDecorator : IDataSource
    {
        private readonly IDataSource _dataSource;
        private readonly IDictionary<string, Column> _columns;
        private readonly IFormatProvider _formatProvider;

        public DataSourceTypedColumnsDecorator(IDataSource dataSource, IEnumerable<Column> columns, IFormatProvider formatProvider)
        {
            _dataSource = dataSource;
            _columns = columns.Where(c => !c.DatabaseGenerated).ToDictionary(c => c.Name, c => c);
            _formatProvider = formatProvider;
        }

        public object GetValue(string name) => ConvertValue(_columns[name], _dataSource.GetValue(name), _formatProvider);

        public bool Read() => _dataSource.Read();

        public void Dispose() => _dataSource.Dispose();

        private static object ConvertValue(Column column, object value, IFormatProvider formatProvider)
        {
            if (value == null)
            {
                return column.Nullable ? null : throw new InputArgumentException($"Column \"{column.Name}\" does not allow null values");
            }
            else if (column.Array)
            {
                return value switch
                {
                    string jsonString => NormalizeArray(column, ParseJsonArray(jsonString), formatProvider),
                    IEnumerable valueCollection => NormalizeArray(column, valueCollection, formatProvider),
                    _ => throw new InputArgumentException($"The value received for an array column \"{column.Name}\" is neither an array nor a JSON array string")
                };
            }
            else
            {
                return ConvertSingleValue(column, value, formatProvider);
            }
        }

        private static object ConvertSingleValue(Column column, object value, IFormatProvider formatProvider) =>
            column.Type switch
            {
                ColumnType.Boolean => ToBool(value),
                ColumnType.Double => ToDouble(value, formatProvider),
                ColumnType.Integer => ToInt(value),
                ColumnType.Long => ToLong(value),
                ColumnType.Text => ToString(value),
                ColumnType.String => ToString(value),
                ColumnType.DateTime => ToDateTime(value, formatProvider),
                ColumnType.Guid => ToGuid(value),
                ColumnType.Json => value,
                ColumnType.Vector => value,
                _ => throw new InputArgumentException($"Unknown column type \"{column.Type}\"")
            };

        private static object ToBool(object value) =>
            value switch
            {
                bool => value,
                string stringValue => HandleFormatException(() => bool.Parse(stringValue), value, typeof(bool)),
                _ => throw CreateConvertException(value, typeof(bool))
            };

        private static object ToInt(object value) =>
            value switch
            {
                int intValue => intValue,
                short shortValue => (int)shortValue,
                long longValue => (int)longValue,
                ushort ushortValue => (int)ushortValue,
                uint uintValue => (int)uintValue,
                ulong ulongValue => (int)ulongValue,
                double doubleValue => (int)doubleValue,
                float floatValue => (int)floatValue,
                decimal decimalValue => (int)decimalValue,
                string stringValue => (int)HandleFormatException(() => int.Parse(stringValue), value, typeof(int)),
                _ => throw CreateConvertException(value, typeof(int))
            };

        private static object ToLong(object value) =>
            value switch
            {
                long longValue => longValue,
                short shortValue => (long)shortValue,
                int intValue => (long)intValue,
                ushort ushortValue => (long)ushortValue,
                uint uintValue => (long)uintValue,
                ulong ulongValue => (long)ulongValue,
                double doubleValue => (long)doubleValue,
                float floatValue => (long)floatValue,
                decimal decimalValue => (long)decimalValue,
                string stringValue => (long)HandleFormatException(() => long.Parse(stringValue), value, typeof(long)),
                _ => throw CreateConvertException(value, typeof(long))
            };

        private static object ToDouble(object value, IFormatProvider formatProvider)
        {
            double result = value switch
            {
                double doubleValue => doubleValue,
                short shortValue => (double)shortValue,
                int intValue => (double)intValue,
                long longValue => (double)longValue,
                ushort ushortValue => (double)ushortValue,
                uint uintValue => (double)uintValue,
                ulong ulongValue => (double)ulongValue,
                float floatValue => (double)floatValue,
                decimal decimalValue => (double)decimalValue,
                string stringValue => (double)HandleFormatException(() => double.Parse(stringValue, formatProvider), value, typeof(double)),
                _ => throw CreateConvertException(value, typeof(double))
            };

            //Assuming there is no sense to store NaN values in the database
            return double.IsNaN(result) ? null : result;
        }

        private static object ToDateTime(object value, IFormatProvider formatProvider) =>
            value switch
            {
                DateTime => value,
                string stringValue => HandleFormatException(() => DateTime.Parse(stringValue, formatProvider), value, typeof(DateTime)),
                _ => throw CreateConvertException(value, typeof(DateTime))
            };

        private static object ToGuid(object value) =>
            value switch
            {
                Guid => value,
                string stringValue => HandleFormatException(() => Guid.Parse(stringValue), value, typeof(Guid)),
                _ => throw CreateConvertException(value, typeof(Guid))
            };

        private static object ToString(object value) =>
            value switch
            {
                DateTime dateTimeValue => dateTimeValue.ToSortableString(),
                string stringValue => stringValue,
                _ => value.ToString()
            };

        private static object HandleFormatException(Func<object> parseFunction, object value, Type targetType)
        {
            try
            {
                return parseFunction();
            }
            catch (FormatException)
            {
                throw CreateConvertException(value, targetType);
            }
        }

        private static Exception CreateConvertException(object value, Type targetType) =>
            new InputArgumentException($"A value \"{value}\" of type \"{value.GetType()}\" can't be converted to \"{targetType}\"");

        private static object NormalizeArray(Column column, IEnumerable valueCollection, IFormatProvider formatProvider) =>
            valueCollection.Cast<object>().Select(v => ConvertSingleValue(column, v, formatProvider)).ToArray();

        private static object GetJsonNodeValue(JsonNode node) =>
            node switch
            {
                JsonValue value => value.GetValueKind() switch
                {
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    JsonValueKind.Number => value.GetValue<double>(),
                    _ => value.ToString()
                },
                _ => throw new InputArgumentException("An array column can store only primitive values")
            };

        private static IEnumerable<object> ParseJsonArray(string jsonString)
        {
            try
            {
                if (JsonNode.Parse(jsonString) is not JsonArray jsonArray)
                {
                    throw new InputArgumentException($"The string \"{jsonString}\" is not a valid JSON array definition");
                }

                return jsonArray.Select(GetJsonNodeValue).ToArray();
            }
            catch
            {
                throw new InputArgumentException($"The string \"{jsonString}\" is not a valid JSON array definition");
            }
        }
    }
}
