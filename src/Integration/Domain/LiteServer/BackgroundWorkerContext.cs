using System;
using Vertica.Integration.Infrastructure.IO;

namespace Vertica.Integration.Domain.LiteServer
{
	public class BackgroundWorkerContext
	{
		internal BackgroundWorkerContext(IConsoleWriter console)
		{
		    if (console == null) throw new ArgumentNullException(nameof(console));

		    Console = console;
		}

	    /// <summary>
		/// Specifies the number of times the method has been invoked.
		/// </summary>
		public uint InvocationCount { get; private set; }

        internal BackgroundWorkerContext Increment()
        {
            InvocationCount++;

            return this;
        }

        /// <summary>
        /// Gives access to the <see cref="IConsoleWriter"/>.
        /// </summary>
	    public IConsoleWriter Console { get; }

	    /// <summary>
		/// Use this method to signal that you no longer want your method to be invoked.
		/// </summary>
		public BackgroundWorkerContinuation Exit()
		{
            return new BackgroundWorkerContinuation();
        }

        /// <summary>
        /// Use this method to signal how long you want to wait until we invoke your method again.
        /// </summary>
	    public BackgroundWorkerContinuation Wait(TimeSpan time)
        {
            return new BackgroundWorkerContinuation(time);
        }
	}
}