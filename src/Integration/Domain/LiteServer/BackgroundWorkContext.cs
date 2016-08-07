using System;
using System.Threading;

namespace Vertica.Integration.Domain.LiteServer
{
	public struct BackgroundWorkContext
	{
		private readonly Func<TimeSpan> _exit;

		internal BackgroundWorkContext(CancellationToken cancellationToken, uint invocationCount, Func<TimeSpan> exit)
		{
			if (exit == null) throw new ArgumentNullException(nameof(exit));

			CancellationToken = cancellationToken;
			InvocationCount = invocationCount;
			_exit = exit;
		}

		public CancellationToken CancellationToken { get; }

		/// <summary>
		/// Specifies the number of times the method has been invoked.
		/// </summary>
		public uint InvocationCount { get; }

		/// <summary>
		/// Use this method to signal that you want to stop having your method repeated.
		/// </summary>
		public TimeSpan Exit()
		{
			return _exit();
		}
	}
}