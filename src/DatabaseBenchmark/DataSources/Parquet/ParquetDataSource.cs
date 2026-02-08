using DatabaseBenchmark.Common;
using DatabaseBenchmark.DataSources.Interfaces;
using Parquet;
using Parquet.Data;

namespace DatabaseBenchmark.DataSources.Parquet
{
    public sealed class ParquetDataSource : IDataSource
    {
        private const string ListPathMarker = "/list/element";

        private readonly string _filePath;

        private Stream _fileStream;
        private ParquetReader _reader;
        private DataColumn[] _columns;
        private int _currentRowGroupIndex = -1;
        private int _currentGroupRowIndex = -1;
        private int _rowsInCurrentGroup = 0;

        // Maps base column name to column info (for both plain and list columns)
        private Dictionary<string, ColumnInfo> _columnMap;

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

            if (!_columnMap.TryGetValue(name, out var columnInfo))
            {
                throw new InputArgumentException($"Column \"{name}\" not found in Parquet file.");
            }

            if (columnInfo.IsList)
            {
                return GetListValue(columnInfo);
            }
            else
            {
                return columnInfo.Column.Data.GetValue(_currentGroupRowIndex);
            }
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
                _columnMap = new Dictionary<string, ColumnInfo>();

                foreach (var field in _reader.Schema.GetDataFields())
                {
                    var column = groupReader.ReadColumnAsync(field).GetAwaiter().GetResult();
                    columns.Add(column);

                    var fieldPath = field.Path.ToString();
                    var isList = fieldPath.Contains(ListPathMarker);
                    var baseName = isList
                        ? fieldPath.Substring(0, fieldPath.IndexOf(ListPathMarker))
                        : field.Name;

                    var columnInfo = new ColumnInfo
                    {
                        Column = column,
                        IsList = isList,
                        BaseName = baseName
                    };

                    if (isList)
                    {
                        columnInfo.RowBoundaries = ComputeRowBoundaries(column);
                    }

                    _columnMap[baseName] = columnInfo;
                }

                _columns = columns.ToArray();

                // For row count, use the first non-list column, or compute from list boundaries
                _rowsInCurrentGroup = ComputeRowCount();
            }

            _currentGroupRowIndex = 0;
            return true;
        }

        private object GetListValue(ColumnInfo columnInfo)
        {
            var boundaries = columnInfo.RowBoundaries;
            var startIndex = boundaries[_currentGroupRowIndex];
            var endIndex = _currentGroupRowIndex + 1 < boundaries.Length
                ? boundaries[_currentGroupRowIndex + 1]
                : columnInfo.Column.Data.Length;

            var length = endIndex - startIndex;
            var elementType = columnInfo.Column.Field.ClrType;
            var result = Array.CreateInstance(elementType, length);

            for (int i = 0; i < length; i++)
            {
                result.SetValue(columnInfo.Column.Data.GetValue(startIndex + i), i);
            }

            return result;
        }

        private static int[] ComputeRowBoundaries(DataColumn column)
        {
            var repLevels = column.RepetitionLevels;
            if (repLevels == null || repLevels.Length == 0)
            {
                // No repetition levels means single-element arrays or empty
                // Each element is its own row
                var boundaries = new int[column.Data.Length];
                for (int i = 0; i < boundaries.Length; i++)
                {
                    boundaries[i] = i;
                }

                return boundaries;
            }

            // RepetitionLevel 0 indicates start of a new row
            var rowStarts = new List<int>();
            for (int i = 0; i < repLevels.Length; i++)
            {
                if (repLevels[i] == 0)
                {
                    rowStarts.Add(i);
                }
            }

            return rowStarts.ToArray();
        }

        private int ComputeRowCount()
        {
            // First try non-list columns
            foreach (var info in _columnMap.Values)
            {
                if (!info.IsList)
                {
                    return info.Column.Data.Length;
                }
            }

            // If all columns are list columns, use row boundaries count
            foreach (var info in _columnMap.Values)
            {
                if (info.IsList && info.RowBoundaries != null)
                {
                    return info.RowBoundaries.Length;
                }
            }

            return 0;
        }

        private class ColumnInfo
        {
            public DataColumn Column { get; set; }

            public bool IsList { get; set; }
            
            public string BaseName { get; set; }
            
            public int[] RowBoundaries { get; set; }
        }
    }
}
