using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Vertica.Integration.Infrastructure.Parsing
{
	public class CsvRow : DynamicObject
	{
	    private readonly string[] _data;
	    private readonly IDictionary<string, int> _headers;

	    public CsvRow(string[] data, IDictionary<string, int> headers = null)
		{
			_headers = headers;
			_data = data;
		}

	    public string this[string name]
	    {
	        get
	        {
	            AssertHeaders();

	            int index;
	            if (_headers.TryGetValue(name, out index))
	                return _data[index];

	            return null;
	        }
	    }

	    public string this[int index]
	    {
	        get
	        {
	            if (index < 0 || index >= _data.Length)
	                return null;

	            return _data[index];
	        }
	    }

	    public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
            AssertHeaders();

			int index;
			if (!_headers.TryGetValue(binder.Name, out index))
				throw new InvalidOperationException(String.Format("Row does not contain column with name '{0}'.", binder.Name));

			result = _data[index];

			return true;
		}

	    private void AssertHeaders()
	    {
	        if (_headers == null)
	            throw new InvalidOperationException("No headers preset.");
	    }
	}
}
