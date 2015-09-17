using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Windsor;

namespace Vertica.Integration
{
	public class ExtensibilityConfiguration
	{
		private readonly Dictionary<Type, object> _extensions;

		internal ExtensibilityConfiguration()
		{
			_extensions = new Dictionary<Type, object>();
		}

		public T Register<T>(Func<T> factory) where T : class
		{
			if (factory == null) throw new ArgumentNullException("factory");

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

		internal IEnumerable<IInitializable<IWindsorContainer>> ContainerInitializations
		{
			get { return _extensions.Values.OfType<IInitializable<IWindsorContainer>>(); }
		}

		internal IEnumerable<IDisposable> Disposers
		{
			get { return _extensions.Values.OfType<IDisposable>(); }
		}
	}
}