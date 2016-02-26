using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vertica.Utilities_v4.Comparisons;

namespace Vertica.Integration.Model
{
    public class Arguments : IEnumerable<KeyValuePair<string, string>>
    {
	    private readonly string _prefix;
	    private readonly KeyValuePair<string, string>[] _pairs; 
        private readonly Dictionary<string, string> _dictionary;

	    public Arguments()
			: this(new string[0])
	    {
	    }

        public Arguments(params string[] values)
            : this((values ?? new string[0]).Select(x => new KeyValuePair<string, string>(x, x)).ToArray())
        {
        }

	    public Arguments(params KeyValuePair<string, string>[] pairs)
			: this(null, pairs)
	    {
	    }

	    internal Arguments(string prefix, params KeyValuePair<string, string>[] pairs)
        {
		    _prefix = prefix ?? string.Empty;

		    var uniqueKeys = Eq<KeyValuePair<string, string>>.By(
                (x, y) => string.Equals(x.Key, y.Key, StringComparison.OrdinalIgnoreCase),
                x => x.Key.ToLowerInvariant().GetHashCode());

            _pairs = (pairs ?? new KeyValuePair<string, string>[0]).Distinct(uniqueKeys).ToArray();
            _dictionary = _pairs.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
        }

        public bool Contains(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(key));

            return _dictionary.ContainsKey(key);
        }

        public string this[int index] => _pairs[index].Key;

	    public string this[string key]
        {
	        get
	        {
		        string value;
		        TryGetValue(key, out value);

		        return value;
	        }
        }

	    public int Length => _pairs.Length;

	    public bool TryGetValue(string key, out string value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(key));

            return _dictionary.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _pairs.OfType<KeyValuePair<string, string>>().GetEnumerator();
        }

	    public override string ToString()
	    {
			return string.Join(" ", _pairs.Select(x => $"{_prefix}{x.Key}{(string.IsNullOrWhiteSpace(x.Value) || string.Equals(x.Key, x.Value) ? string.Empty : string.Concat(":", x.Value))}"));
	    }

	    public static Arguments Empty => new Arguments(new string[0]);
    }
}