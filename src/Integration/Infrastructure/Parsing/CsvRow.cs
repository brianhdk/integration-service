using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Vertica.Integration.Infrastructure.Parsing
{
	public class CsvRow : DynamicObject
	{
	    private readonly string[] _data;
	    private readonly IDictionary<string, int> _headers;
	    private readonly string _delimiter;

	    public CsvRow(string[] data, IDictionary<string, int> headers = null, string delimiter = ";")
		{
			_headers = headers;
	        _delimiter = delimiter;
	        _data = data;

            if (headers != null && data.Length != headers.Count)
                throw new ArgumentException(
                    String.Format("Data has {0} elements but we expected {1} elements (= headers).", data.Length, headers.Count));
		}

	    public string this[string name]
	    {
	        get
	        {
	            return _data[GetIndexByName(name)];
	        }
	        set
	        {
	            _data[GetIndexByName(name)] = value;
	        }
	    }

	    private int GetIndexByName(string name)
	    {
            if (_headers == null)
                throw new InvalidOperationException("Row was not initialized with headers.");

	        int index;
	        if (!_headers.TryGetValue(name, out index))
	            throw new ArgumentException(String.Format("Could not find any header named '{0}'.", name));

	        return index;
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

	    public override string ToString()
	    {
	        return String.Join(_delimiter, _data);
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
	}
}