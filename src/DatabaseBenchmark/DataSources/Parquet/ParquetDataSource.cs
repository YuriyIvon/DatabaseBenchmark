using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using Parquet;
using Parquet.Data;

namespace DatabaseBenchmark.DataSources.Parquet
{
    public sealed class ParquetDataSource : IDataSource
    {
        private readonly string _filePath;

        private Stream _fileStream;
        private ParquetReader _reader;
        private DataColumn[] _columns;
        private int _currentRowGroupIndex = -1;
        private int _currentGroupRowIndex = -1;
        private int _rowsInCurrentGroup = 0;

        public ParquetDataSource(string filePath)
        {
            _filePath = Path.GetFullPath(filePath);
        }

        public void Dispose()
        {
            _reader?.Dispose();
            _fileStream?.Dispose();
        }

        public object GetValue(string name)
        {
            if (_columns == null)
            {
                throw new InvalidOperationException("No data has been read yet. Call Read() first.");
            }

            var column = _columns.FirstOrDefault(c => c.Field.Name == name);
            if (column == null)
            {
                throw new ArgumentException($"Column '{name}' not found in Parquet file.");
            }

            return column.Data.GetValue(_currentGroupRowIndex);
        }

        public bool Read()
        {
            if (_reader == null)
            {
                Open();
            }

            _currentGroupRowIndex++;

            if (_currentGroupRowIndex >= _rowsInCurrentGroup)
            {
                if (!LoadNextRowGroup())
                {
                    return false;
                }
            }

            return true;
        }

        private void Open()
        {
            _fileStream = File.OpenRead(_filePath);
            _reader = ParquetReader.CreateAsync(_fileStream).GetAwaiter().GetResult();
        }

        private bool LoadNextRowGroup()
        {
            _currentRowGroupIndex++;

            if (_currentRowGroupIndex >= _reader.RowGroupCount)
            {
                return false;
            }

            using (var groupReader = _reader.OpenRowGroupReader(_currentRowGroupIndex))
            {
                var columns = new List<DataColumn>();
                foreach (var field in _reader.Schema.GetDataFields())
                {
                    var column = groupReader.ReadColumnAsync(field).GetAwaiter().GetResult();
                    columns.Add(column);
                }

                _columns = columns.ToArray();
                _rowsInCurrentGroup = _columns.Length > 0 ? _columns[0].Data.Length : 0;
            }

            _currentGroupRowIndex = 0;
            return true;
        }
    }
}
