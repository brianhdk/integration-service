using System;

namespace Vertica.Integration.Domain.LiteServer
{
	public class BackgroundWorkerContext
	{
		internal BackgroundWorkerContext(uint invocationCount)
		{
			InvocationCount = invocationCount;
		}

		/// <summary>
		/// Specifies the number of times the method has been invoked.
		/// </summary>
		public uint InvocationCount { get; }

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