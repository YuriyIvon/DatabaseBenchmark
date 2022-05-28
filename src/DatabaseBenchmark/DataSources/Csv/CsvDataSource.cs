using CsvHelper;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using System.Globalization;

namespace DatabaseBenchmark.DataSources.Csv
{
    public class CsvDataSource : IDataSource
    {
        private readonly string _filePath;
        private readonly Dictionary<string, Type> _columnTypes;

        private StreamReader _streamReader;
        private CsvReader _reader;

        public CsvDataSource(string filePath, Table table)
        {
            _filePath = filePath;
            _columnTypes = table.Columns.ToDictionary(c => c.Name, c => GetColumnType(c.Type));
        }

        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_streamReader != null)
            {
                _streamReader.Dispose();
            }
        }

        public object GetValue(string name) =>
            _columnTypes.TryGetValue(name, out var type) 
                ? NaNToNull(_reader.GetField(type, name)) 
                : _reader.GetField<string>(name);

        public bool Read()
        {
            if (_reader == null)
            {
                Open();
            }

            return _reader.Read();
        }

        private void Open()
        {
            _streamReader = new StreamReader(_filePath);
            _reader = new CsvReader(_streamReader, CultureInfo.InvariantCulture);

            _reader.Read();
            _reader.ReadHeader();
        }

        private static Type GetColumnType(ColumnType columnType) =>
            columnType switch
            {
                ColumnType.Boolean => typeof(bool),
                ColumnType.Double => typeof(double),
                ColumnType.Integer => typeof(int),
                ColumnType.Text => typeof(string),
                ColumnType.String => typeof(string),
                ColumnType.DateTime => typeof(DateTime),
                ColumnType.Guid => typeof(Guid),
                _ => throw new InputArgumentException($"Unknown column type \"{columnType}\"")
            };

        public static object NaNToNull(object value) =>
            value is double doubleValue && double.IsNaN(doubleValue) ? null : value;
    }
}
