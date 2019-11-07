using System.Threading;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;
using Task = System.Threading.Tasks.Task;

namespace Vertica.Integration.Slack.Bot.Commands
{
    internal class RunTaskCommand : ISlackBotCommand
    {
        private readonly ITaskRunner _runner;
        private readonly ITaskFactory _factory;

        public RunTaskCommand(ITaskRunner runner, ITaskFactory factory)
        {
            _runner = runner;
            _factory = factory;
        }

        public bool TryHandle(SlackBotCommandContext context, CancellationToken token, out Task task)
        {
            task = null;

            string taskName = context.IncomingMessage.Text;

            ITask taskToExecute;
            if (!string.IsNullOrWhiteSpace(taskName) && _factory.TryGet(taskName, out taskToExecute))
            {
                task = context.WriteText($"Task {taskToExecute.Name()} is being executed.")
                    .ContinueWith(t => Task.FromResult(_runner.Execute(taskToExecute)), token)
                    .ContinueWith(t => context.WriteText($"Task {taskToExecute.Name()} finished executing."), token);
            }

            return task != null;
        }
    }
}