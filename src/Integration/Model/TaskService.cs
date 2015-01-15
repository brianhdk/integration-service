using System;
using System.Collections.Generic;
using System.Linq;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model.Exceptions;
using Vertica.Integration.Model.Startup;

namespace Vertica.Integration.Model
{
    public class TaskService : ITaskService
    {
        private readonly ITaskFactory _factory;
        private readonly StartupAction[] _actions;

        public TaskService(ITaskFactory factory, ITaskRunner runner, ILogger logger)
        {
            _factory = factory;

            _actions = new StartupAction[]
            {
                new StartWebApiHost(logger),
                new ExecuteTask(runner),
                new ExecuteTaskFromWebServiceInConsole(logger),
                new ExecuteTaskInWindowsService(runner, logger),
                new InstallWindowsServiceTaskHost(),
                new UninstallWindowsServiceTaskHost()
            };
        }

        public void Execute(string taskName, params string[] arguments)
        {
            if (String.IsNullOrWhiteSpace(taskName)) throw new ArgumentException(@"Value cannot be null or empty.", "taskName");

            ITask task = _factory.GetTaskByName(taskName);

            if (task == null) throw new TaskNotFoundException(taskName);

            var context = new ExecutionContext(taskName, task, arguments);

            StartupAction action = _actions.FirstOrDefault(x => x.IsSatisfiedBy(context));

            if (action == null)
                throw new StartupActionNotFoundException(arguments);

            action.Execute(context);
        }

        public IEnumerable<ITask> GetAll()
        {
            return (_factory.GetTasks() ?? Enumerable.Empty<ITask>()).Distinct().ToList();
        }
    }
}