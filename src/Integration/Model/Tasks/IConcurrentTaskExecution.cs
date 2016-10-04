using System;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Model.Tasks
{
    public interface IConcurrentTaskExecution
    {
        IDisposable Handle(ITask task, TaskLog log);
    }
}