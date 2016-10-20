using System;
using Vertica.Integration.Infrastructure.IO;

namespace Vertica.Integration.Domain.LiteServer
{
    // TODO: Tilføj en ILog ala den der findes på TaskExecutionContext
    // TODO: Tilføj en Writer - IConsoleWriter
	public class BackgroundWorkerContext
	{
		internal BackgroundWorkerContext(uint invocationCount, IConsoleWriter writer)
		{
		    if (writer == null) throw new ArgumentNullException(nameof(writer));

		    InvocationCount = invocationCount;
		    Writer = writer;
		}

	    /// <summary>
		/// Specifies the number of times the method has been invoked.
		/// </summary>
		public uint InvocationCount { get; }

        /// <summary>
        /// Gives access to the <see cref="IConsoleWriter"/>.
        /// </summary>
	    public IConsoleWriter Writer { get; }

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