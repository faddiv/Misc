using System;
using System.Collections;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace ExportSerializationHelper
{
    public class ExportRowReader : IDataReader
    {
        private readonly SourceReader _sourceReader;
        private readonly IEnumerable _source;
        private int _rowIndex;
        private IEnumerator? _activeEnumerator;

        public ExportRowReader(SourceReader reader, IEnumerable source)
        {
            _sourceReader = reader ?? throw new ArgumentNullException(nameof(reader));
            _source = source ?? throw new ArgumentNullException(nameof(source));
            Data = new object[_sourceReader.CountFields];
            RowIndex = -1;
        }

        public int FieldCount => _sourceReader.CountFields;

        public int RowIndex { get; private set; }

        public object Original { get; }

        public object[] Data { get; }

        public object this[int i] => GetValueInternal(i);

        public object this[string name] => GetValueInternal(GetOrdinalInternal(name));

        public int Depth => 0;

        public bool IsClosed => _activeEnumerator == null;

        public int RecordsAffected => 0;

        public void Close()
        {
            if (_activeEnumerator == null)
                return;

            if(_activeEnumerator is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _activeEnumerator = null;
        }

        public void Dispose()
        {
            Close();
        }

        public bool GetBoolean(int i)
        {
            return Convert.ToBoolean(GetValueInternal(i));
        }

        public byte GetByte(int i)
        {
            return Convert.ToByte(GetValueInternal(i));
        }

        public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return Convert.ToChar(GetValueInternal(i));
        }

        public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            return Convert.ToDateTime(GetValueInternal(i));
        }

        public decimal GetDecimal(int i)
        {
            return Convert.ToDecimal(GetValueInternal(i));
        }

        public double GetDouble(int i)
        {
            return Convert.ToDouble(GetValueInternal(i));
        }

        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            return Convert.ToSingle(GetValueInternal(i));
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            return Convert.ToInt16(GetValueInternal(i));
        }

        public int GetInt32(int i)
        {
            return Convert.ToInt32(GetValueInternal(i));
        }

        public long GetInt64(int i)
        {
            return Convert.ToInt64(GetValueInternal(i));
        }

        public string GetName(int i)
        {
            var headers = _sourceReader.GetHeader();
            return headers[i];
        }

        public int GetOrdinal(string name)
        {
            return GetOrdinalInternal(name);
        }

        public DataTable? GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            return Convert.ToString(GetValueInternal(i)) ?? "";
        }

        public object GetValue(int i)
        {
            return GetValueInternal(i);
        }

        public int GetValues(object[] values)
        {
            if (values is null) throw new ArgumentNullException(nameof(values));

            var count = Math.Min(FieldCount, values.Length);
            Array.Copy(Data, values, count);
            return count;
        }

        public bool IsDBNull(int i)
        {
            return GetValueInternal(i) == DBNull.Value;
        }

        public bool NextResult()
        {
            Close();
            return false;
        }

        public bool Read()
        {
            if (_activeEnumerator == null)
            {
                _activeEnumerator = _source.GetEnumerator();
            }
            if (!_activeEnumerator.MoveNext())
            {
                return false;
            }
            var item = _activeEnumerator.Current;
            for (int i = 0; i < _sourceReader.CountFields; i++)
            {
                Data[i] = _sourceReader.GetValue(item, i) ?? DBNull.Value;
            }
            RowIndex++;
            return true;
        }

        private int GetOrdinalInternal(string name)
        {
            var headers = _sourceReader.GetHeader();
            for (int i = 0; i < FieldCount; i++)
            {
                if (string.Equals(headers[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }


        private object GetValueInternal(int i)
        {
            return Data[i];
        }

    }
}
