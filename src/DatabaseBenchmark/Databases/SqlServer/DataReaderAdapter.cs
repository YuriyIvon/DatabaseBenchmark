﻿using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseBenchmark.Databases.SqlServer
{
    public sealed class DataReaderAdapter : IDataReader
    {
        private readonly IDataSource _dataSource;
        private readonly Table _table;
        private readonly Dictionary<string, Type> _columnTypes;

        public DataReaderAdapter(IDataSource dataSource, Table table)
        {
            _dataSource = dataSource;
            _table = table;
            _columnTypes = table.Columns.ToDictionary(c => c.Name, c => c.GetNativeType());
        }

        public object this[int i] => this[_table.Columns[i].Name];

        public object this[string name] =>
            _columnTypes.TryGetValue(name, out var type)
                ? _dataSource.GetValue(type, name)
                : _dataSource.GetValue(typeof(string), name);

        public int Depth => throw new NotSupportedException();

        public bool IsClosed => throw new NotSupportedException();

        public int RecordsAffected => throw new NotSupportedException();

        public int FieldCount => _table.Columns.Length;

        public void Close() => _dataSource.Dispose();

        public void Dispose() => _dataSource.Dispose();

        public bool GetBoolean(int i) => throw new NotSupportedException();

        public byte GetByte(int i) => throw new NotSupportedException();

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) =>
            throw new NotSupportedException();

        public char GetChar(int i) => throw new NotSupportedException();

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) =>
            throw new NotSupportedException();

        public IDataReader GetData(int i) => throw new NotSupportedException();

        public string GetDataTypeName(int i) => throw new NotSupportedException();

        public DateTime GetDateTime(int i) => throw new NotSupportedException();

        public decimal GetDecimal(int i) => throw new NotSupportedException();

        public double GetDouble(int i) => throw new NotSupportedException();

        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
        public Type GetFieldType(int i) => this[i].GetType();

        public float GetFloat(int i) => throw new NotSupportedException();

        public Guid GetGuid(int i) => throw new NotSupportedException();

        public short GetInt16(int i) => throw new NotSupportedException();

        public int GetInt32(int i) => throw new NotSupportedException();

        public long GetInt64(int i) => throw new NotSupportedException();

        public string GetName(int i) => _table.Columns[i].Name;

        public int GetOrdinal(string name) =>
            Enumerable.Range(0, _table.Columns.Length)
                .FirstOrDefault(i => _table.Columns[i].Name == name);

        public DataTable GetSchemaTable() => throw new NotSupportedException();

        public string GetString(int i) => throw new NotSupportedException();

        public object GetValue(int i) => this[i];

        public int GetValues(object[] values) => throw new NotSupportedException();

        public bool IsDBNull(int i) => this[i] == null;

        public bool NextResult() => throw new NotSupportedException();

        public bool Read() => _dataSource.Read();
    }
}
