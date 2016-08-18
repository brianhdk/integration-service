using System;

namespace Vertica.Integration.Domain.LiteServer
{
	public class BackgroundWorkerContext
	{
		private readonly Func<TimeSpan> _exit;

		internal BackgroundWorkerContext(uint invocationCount, Func<TimeSpan> exit)
		{
			if (exit == null) throw new ArgumentNullException(nameof(exit));

			InvocationCount = invocationCount;
			_exit = exit;
		}

		/// <summary>
		/// Specifies the number of times the method has been invoked.
		/// </summary>
		public uint InvocationCount { get; }

		/// <summary>
		/// Use this method to signal that you want to stop having your method invoked.
		/// </summary>
		public TimeSpan Exit()
		{
			return _exit();
		}
	}
}