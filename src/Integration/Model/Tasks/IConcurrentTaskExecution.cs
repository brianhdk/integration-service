using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Model.Tasks
{
    public interface IConcurrentTaskExecution
    {
        ConcurrentTaskExecutionResult Handle(ITask task, Arguments arguments, TaskLog log);
    }
}