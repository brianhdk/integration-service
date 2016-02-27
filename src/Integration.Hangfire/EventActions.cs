using System;
using System.Collections;
using System.Collections.Generic;
using Castle.MicroKernel;

namespace Vertica.Integration.Hangfire
{
	public abstract class EventActions<TEvent> : IEnumerable<Action<IKernel>>
		where TEvent : EventActions<TEvent>
	{
		private readonly List<Action<IKernel>> _list;

		protected EventActions()
		{
			_list = new List<Action<IKernel>>();
		}

		public TEvent Add(Action<IKernel> action)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			_list.Add(action);

			return This;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<Action<IKernel>> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		protected abstract TEvent This { get; }
	}
}