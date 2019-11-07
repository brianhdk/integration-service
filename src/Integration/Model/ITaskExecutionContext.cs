using System;
using System.Threading;

namespace Vertica.Integration.Model
{
    public interface ITaskExecutionContext<out TWorkItem> : ITaskExecutionContext
    {
        TWorkItem WorkItem { get; }
    }

    public interface ITaskExecutionContext
    {
        /// <summary>
        /// Returns the unique Id of the initial logging entry (if logging is enabled), created when the execution of this task was started.
        /// </summary>
        string TaskLogId { get; }

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

        /// <summary>
        /// Typed Property bag - internally uses the weakly typed property bag.
        /// </summary>
        T TypedBag<T>(string name, T value = null) where T : class;
    }
}