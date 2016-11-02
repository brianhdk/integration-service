using System;
using Vertica.Integration.Infrastructure.IO;

namespace Vertica.Integration.Domain.LiteServer
{
    // TODO: Tilføj en ILog ala den der findes på TaskExecutionContext
	public class BackgroundWorkerContext
	{
		internal BackgroundWorkerContext(uint invocationCount, IConsoleWriter console)
		{
		    if (console == null) throw new ArgumentNullException(nameof(console));

		    InvocationCount = invocationCount;
		    Console = console;
		}

	    /// <summary>
		/// Specifies the number of times the method has been invoked.
		/// </summary>
		public uint InvocationCount { get; }

        /// <summary>
        /// Gives access to the <see cref="IConsoleWriter"/>.
        /// </summary>
	    public IConsoleWriter Console { get; }

	    /// <summary>
		/// Use this method to signal that you want to stop having your method invoked.
		/// </summary>
		public BackgroundWorkerContinuation Exit()
		{
            return new BackgroundWorkerContinuation();
        }

        /// <summary>
        /// Use this method to signal 
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
	    public BackgroundWorkerContinuation Wait(TimeSpan time)
        {
            return new BackgroundWorkerContinuation(time);
        }
	}
}