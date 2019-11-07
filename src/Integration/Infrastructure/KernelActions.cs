using System;
using System.Collections;
using System.Collections.Generic;
using Castle.MicroKernel;

namespace Vertica.Integration.Infrastructure
{
	public abstract class KernelActions<TEvent> : IEnumerable<Action<IKernel>>
		where TEvent : KernelActions<TEvent>
	{
		private readonly List<Action<IKernel>> _list;

		protected KernelActions()
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