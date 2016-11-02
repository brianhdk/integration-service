using System;
using System.Collections;
using System.Collections.Generic;

namespace Vertica.Integration
{
	public class ExtensibilityConfiguration : IEnumerable<object>
	{
		private readonly Dictionary<Type, object> _extensions;

		internal ExtensibilityConfiguration()
		{
			_extensions = new Dictionary<Type, object>();
		}

		public T Register<T>(Func<T> factory) where T : class
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));

			T value;
			object cached;
			if (_extensions.TryGetValue(typeof (T), out cached))
			{
				value = (T) cached;
			}
			else
			{
				_extensions[typeof(T)] = value = factory();
			}

			return value;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<object> GetEnumerator()
		{
			return _extensions.Values.GetEnumerator();
		}
	}
}