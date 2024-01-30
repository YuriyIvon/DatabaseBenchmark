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

        private bool _treatBlankAsNull;
        private StreamReader _streamReader;
        private CsvReader _reader;

        public CsvDataSource(
            string filePath,
            IOptionsProvider optionsProvider)
        {
            _filePath = Path.GetFullPath(filePath);
            _optionsProvider = optionsProvider;
        }

        public void Dispose()
        {
            _reader?.Dispose();
            _streamReader?.Dispose();
        }

        public object GetValue(string name)
        {
            var value = _reader.GetField(name);

            return _treatBlankAsNull && value == string.Empty ? null : value;
        }

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
            _treatBlankAsNull = options.TreatBlankAsNull;

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
    }
}
