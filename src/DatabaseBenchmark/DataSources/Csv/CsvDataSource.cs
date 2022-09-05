using CsvHelper;
using CsvHelper.Configuration;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DatabaseBenchmark.DataSources.Csv
{
    public sealed class CsvDataSource : IDataSource
    {
        private readonly string _filePath;
        private readonly IOptionsProvider _optionsProvider;

        private StreamReader _streamReader;
        private CsvReader _reader;

        public CsvDataSource(
            string filePath,
            IOptionsProvider optionsProvider)
        {
            _filePath = filePath;
            _optionsProvider = optionsProvider;
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

        public object GetValue(Type type, string name) =>
            NaNToNull(_reader.GetField(type, name));

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
            var options = _optionsProvider.GetOptions<CsvDataSourceOptions>();
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture);

            if (options.Delimiter != null)
            {
                configuration.Delimiter = Regex.Unescape(options.Delimiter);
            }

            _streamReader = new StreamReader(_filePath);
            _reader = new CsvReader(_streamReader, configuration);

            _reader.Read();
            _reader.ReadHeader();
        }

        public static object NaNToNull(object value) =>
            value is double doubleValue && double.IsNaN(doubleValue) ? null : value;
    }
}
