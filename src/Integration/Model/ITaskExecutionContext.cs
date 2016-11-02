using System;
using System.Threading;

namespace Vertica.Integration.Model
{
    public interface ITaskExecutionContext
    {
        /// <summary>
        /// Returns a point in time <see cref="DateTimeOffset"/> for when the execution of this task was started.
        /// </summary>
        DateTimeOffset StartTimeUtc { get; }

        ILog Log { get; }

        Arguments Arguments { get; }

        CancellationToken CancellationToken { get; }
        void ThrowIfCancelled();

        /// <summary>
        /// Property bag you can use to share state in the TaskExecutionFlow.
        /// This is an add-on to the existing workitem infrastructure.
        /// </summary>
        object this[string name] { get; set; }
    }
}