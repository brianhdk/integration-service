using System;
using System.Collections.Generic;
using Castle.Windsor;

namespace Vertica.Integration
{
	public class ExtensibilityConfiguration
	{
		private readonly Dictionary<Type, object> _cache;
		private readonly List<IInitializable<IWindsorContainer>> _containerInitializations;
		private readonly List<IDisposable> _disposers;

		internal ExtensibilityConfiguration()
		{
			_cache = new Dictionary<Type, object>();
			_containerInitializations = new List<IInitializable<IWindsorContainer>>();
			_disposers = new List<IDisposable>();			
		}

		public ExtensibilityConfiguration Register(object extension)
		{
			if (extension == null) throw new ArgumentNullException("extension");

			var disposer = extension as IDisposable;

			if (disposer != null)
				_disposers.Add(disposer);

			var containerInitializable = extension as IInitializable<IWindsorContainer>;

			if (containerInitializable != null)
				_containerInitializations.Add(containerInitializable);

			return this;
		}

		public T Cache<T>(Func<T> factory) where T : class
		{
			if (factory == null) throw new ArgumentNullException("factory");

			T value;
			object cached;
			if (_cache.TryGetValue(typeof (T), out cached))
			{
				value = (T) cached;
			}
			else
			{
				_cache[typeof(T)] = value = factory();
			}

			return value;
		}

		internal IEnumerable<IInitializable<IWindsorContainer>> ContainerInitializations
		{
			get { return _containerInitializations; }
		}

		internal IEnumerable<IDisposable> Disposers
		{
			get { return _disposers; }
		}
	}
}