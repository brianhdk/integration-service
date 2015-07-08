using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration.Infrastructure.Parsing
{
    public class CsvRow : DynamicObject, IEnumerable<string>
    {
        private readonly string[] _data;
        private readonly IDictionary<string, int> _headers;

        public CsvRow(string[] data, string delimiter = CsvConfiguration.DefaultDelimiter, IDictionary<string, int> headers = null, uint? lineNumber = null)
        {
            if (data == null) throw new ArgumentNullException("data");

            if (headers != null && headers.Count > 0 && data.Length != headers.Count)
                throw new ArgumentException(
                    String.Format(
                        "Row{0} has {1} columns but we expected {2} columns (equal to number of header columns).",
                        lineNumber.HasValue ? String.Concat(" #", lineNumber.Value) : String.Empty,
                        data.Length,
                        headers.Count));

            _data = data;
            _headers = headers;
            Meta = new CsvRowMetadata(this, delimiter, lineNumber);
        }

        public string this[string name]
        {
            get { return _data[GetIndexByName(name)]; }
            set { _data[GetIndexByName(name)] = value; }
        }

        public string this[int index]
        {
            get
            {
                if (index < 0 || index >= _data.Length)
                    throw new IndexOutOfRangeException();

                return _data[index];
            }
            set
            {
                if (index < 0 || index >= _data.Length)
                    throw new IndexOutOfRangeException();

                _data[index] = value;
            }
        }

        public int Length
        {
            get { return _data.Length; }
        }

        public bool IsEmpty
        {
            get { return _data.All(String.IsNullOrEmpty); }
        }

        public CsvRowMetadata Meta { get; private set; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _data.Select(Escape).GetEnumerator();
        }

        private int GetIndexByName(string name)
        {
            if (_headers == null)
                throw new InvalidOperationException(
                    String.Format("Row was not initialized with headers."));

            int index;
            if (!_headers.TryGetValue(name, out index))
                throw new ArgumentException(
                    String.Format("Could not find any header named '{0}'.", name));

            return index;
        }

        public override string ToString()
        {
            return String.Join(Meta.Delimiter, this);
        }

        internal string Escape(string data)
        {
            const string doubleQuote = @"""""";

            data = new StringBuilder(data)
                .Replace("\"", "\"\"")
                .Replace("“", doubleQuote)
                .Replace("”", doubleQuote)
                .Replace("„", doubleQuote)
                .ToString();

            if (data.IndexOf(Meta.Delimiter, StringComparison.OrdinalIgnoreCase) >= 0 || data.Contains('"') || data.Contains(Environment.NewLine))
            {
                data = String.Format("\"{0}\"", data);
            }

            return data;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this[binder.Name];

            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            this[binder.Name] = value != null ? value.ToString() : null;

            return true;
        }

        public static CsvRow[] From<T>(IEnumerable<T> elements, Action<ICsvRowBuilderConfiguration> configure = null)
        {
            if (elements == null) throw new ArgumentNullException("elements");

            var headers = typeof (T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.CanRead)
                .ToArray();

            var builder = BeginRows(headers.Select(x => x.Name).ToArray());

            if (configure != null)
                builder.Configure(configure);

            foreach (var element in elements)
            {
                builder.Add(headers
                    .Select(x => x.GetValue(element))
                    .Select(x => x != null ? x.ToString() : null)
                    .ToArray());
            }

            return builder.End().ToRows();
        }

        public static ICsvRowBuilder BeginRows(params string[] headers)
        {
            return new CsvRowBuilder(headers);
        }

        public class CsvRowMetadata
        {
            private readonly CsvRow _row;

            internal CsvRowMetadata(CsvRow row, string delimiter, uint? lineNumber = null)
            {
                if (row == null) throw new ArgumentNullException("row");
                if (String.IsNullOrWhiteSpace(delimiter))
                    throw new ArgumentException(@"Value cannot be null or empty.", "delimiter");

                _row = row;

                if (_row._headers != null)
                    Headers = new CsvRowHeaders(this);

                Delimiter = delimiter;
                LineNumber = lineNumber;
            }

            public CsvRowHeaders Headers { get; private set; }
            public string Delimiter { get; private set; }
            public uint? LineNumber { get; private set; }

            public class CsvRowHeaders : IEnumerable<string>
            {
                private readonly CsvRowMetadata _metadata;

                internal CsvRowHeaders(CsvRowMetadata metadata)
                {
                    if (metadata == null) throw new ArgumentNullException("metadata");
                    if (metadata._row._headers == null) throw new ArgumentException(@"No headers present.", "metadata");

                    _metadata = metadata;
                }

                public int Length
                {
                    get { return _metadata._row._headers.Count; }
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }

                public IEnumerator<string> GetEnumerator()
                {
                    return _metadata._row._headers.Keys.Select(x => _metadata._row.Escape(x)).GetEnumerator();
                }

                public override string ToString()
                {
                    return String.Join(_metadata.Delimiter, this);
                }

                public static implicit operator string[](CsvRowHeaders headers)
                {
                    if (headers == null) throw new ArgumentNullException("headers");

                    return headers._metadata._row._headers.Keys.ToArray();
                }
            }
        }

        public class CsvRowBuilder : ICsvRowBuilder, ICsvRowBuilderFinisher, ICsvRowBuilderConfiguration
        {
            private IDictionary<string, int> _headers;
            private readonly List<CsvRow> _rows;
            private string _delimiter;
            private bool _headerInserted;
            private bool _returnHeaderAsRow;

            internal CsvRowBuilder(string[] headers)
            {
                Headers(headers);
                _rows = new List<CsvRow>();
                _delimiter = CsvConfiguration.DefaultDelimiter;
            }

            public ICsvRowBuilderAdder Configure(Action<ICsvRowBuilderConfiguration> configure)
            {
                if (configure != null)
                    configure(this);

                return this;
            }

            public ICsvRowBuilderFinisher Headers(params string[] headers)
            {
                if (_headers != null) throw new InvalidOperationException(@"Headers have already been set.");

                if (headers != null && headers.Length > 0)
                {
                    _headers = headers
                        .Select((x, i) => new { Header = x, Index = i })
                        .ToDictionary(x => x.Header, x => x.Index, StringComparer.OrdinalIgnoreCase);
                }

                return this;
            }

            public ICsvRowBuilderFinisher Add(params string[] data)
            {
                if (data == null) throw new ArgumentNullException("data");

                var lineNumber = _rows.Count + 1 + (_returnHeaderAsRow ? 1 : 0);

                _rows.Add(new CsvRow(data, _delimiter, _headers, (uint) lineNumber));

                return this;
            }

            public ICsvRowBuilderFinisher AddUsingMapper(Action<ICsvRowMapper> mapper)
            {
                if (mapper == null) throw new ArgumentNullException("mapper");
                if (_headers == null) throw new InvalidOperationException(@"No headers were passed so this method is not allowed.");

                var internalMapper = new CsvRowMapper(_headers);

                mapper(internalMapper);

                Add(internalMapper.ToData());

                return this;
            }

            public ICsvRowBuilderFinisher From<T>(IEnumerable<T> elements, Func<T, string[]> createData)
            {
                if (elements == null) throw new ArgumentNullException("elements");
                if (createData == null) throw new ArgumentNullException("createData");

                elements.ForEach(x => Add(createData(x)));

                return this;
            }

            public ICsvRowBuilderFinisher FromUsingMapper<T>(IEnumerable<T> elements, Action<ICsvRowMapper, T> mapper)
            {
                if (elements == null) throw new ArgumentNullException("elements");
                if (mapper == null) throw new ArgumentNullException("mapper");
                if (_headers == null) throw new InvalidOperationException(@"No headers were passed so this method is not allowed.");

                elements.ForEach(x =>
                {
                    var internalMapper = new CsvRowMapper(_headers);

                    mapper(internalMapper, x);

                    Add(internalMapper.ToData());
                });

                return this;
            }

            public ICsvRowBuilderFinisher End()
            {
                return this;
            }

            public int DataRowCount
            {
                get
                {
                    var count = _rows.Count;

                    return _headerInserted ? count - 1 : count;
                }
            }

            public ICsvRowBuilderConfiguration ReturnHeaderAsRow()
            {
                _returnHeaderAsRow = true;

                return this;
            }

            public ICsvRowBuilderConfiguration ChangeDelimiter(string delimiter)
            {
                if (String.IsNullOrWhiteSpace(delimiter))
                    throw new ArgumentException(@"Value cannot be null or empty.", "delimiter");

                _delimiter = delimiter;

                return this;
            }

            public CsvRow[] ToRows()
            {
                if (_headers != null && _returnHeaderAsRow && !_headerInserted)
                {
                    _rows.Insert(0, new CsvRow(_headers.Keys.ToArray(), _delimiter, _headers, 1));
                    _headerInserted = true;
                }

                return _rows.ToArray();
            }

            public override string ToString()
            {
                return String.Join(Environment.NewLine, ToRows().Select(x => x.ToString()));
            }

            public static implicit operator CsvRow[](CsvRowBuilder builder)
            {
                if (builder == null) throw new ArgumentNullException("builder");

                return builder.ToRows();
            }
        }

        public interface ICsvRowBuilderAdder
        {
            int DataRowCount { get; }
            ICsvRowBuilderFinisher Headers(params string[] headers);
            ICsvRowBuilderFinisher Add(params string[] data);
            ICsvRowBuilderFinisher AddUsingMapper(Action<ICsvRowMapper> mapper);
            ICsvRowBuilderFinisher From<T>(IEnumerable<T> elements, Func<T, string[]> createData);
            ICsvRowBuilderFinisher FromUsingMapper<T>(IEnumerable<T> elements, Action<ICsvRowMapper, T> mapper);
            ICsvRowBuilderFinisher End();
        }

        public interface ICsvRowBuilder : ICsvRowBuilderAdder
        {
            ICsvRowBuilderAdder Configure(Action<ICsvRowBuilderConfiguration> configure);
        }

        public interface ICsvRowBuilderConfiguration
        {
            ICsvRowBuilderConfiguration ReturnHeaderAsRow();
            ICsvRowBuilderConfiguration ChangeDelimiter(string delimiter);
        }

        public interface ICsvRowMapper
        {
            ICsvRowMapper Map(string name, string value);
        }

        private class CsvRowMapper : ICsvRowMapper
        {
            private readonly Dictionary<string, string> _data;
            private readonly IDictionary<string, int> _headers;

            public CsvRowMapper(IDictionary<string, int> headers)
            {
                if (headers == null) throw new ArgumentNullException("headers");

                _headers = headers;
                _data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var keys in _headers.Keys)
                    _data[keys] = String.Empty;
            }

            public ICsvRowMapper Map(string name, string value)
            {
                if (!_data.ContainsKey(name))
                    throw new KeyNotFoundException(String.Format(@"Could not find any header named '{0}'.", name));

                _data[name] = value;

                return this;
            }

            public string[] ToData()
            {
                return _headers.OrderBy(x => x.Value).Select(x => _data[x.Key]).ToArray();
            }
        }

        public interface ICsvRowBuilderFinisher : ICsvRowBuilderAdder
        {
            CsvRow[] ToRows();
        }
    }
}