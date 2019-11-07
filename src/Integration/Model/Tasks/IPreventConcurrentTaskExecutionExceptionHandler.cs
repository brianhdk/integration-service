using System;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Model.Tasks
{
    /// <summary>
    /// Implement this interface to decide at runtime what should happen when unable to acquire a Distributed Lock for Tasks marked with <see cref="PreventConcurrentTaskExecutionAttribute"/>.
    /// </summary>
    public interface IPreventConcurrentTaskExecutionExceptionHandler
    {
        Exception OnException(ITask currentTask, TaskLog log, Arguments arguments, Exception exception);
    }
}