using System.Collections.Generic;
using Vertica.Integration;
using Vertica.Integration.Model;

namespace Experiments.Console.Tasks.Obsolete
{
    public static class Demo
    {
        public static void Run()
        {
            using (var context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Disable()))
                .Tasks(tasks => tasks
                    .AddFromAssemblyOfThis<TaskWithoutSteps>()
                    .Task<TaskWithSteps, TaskThatHasStepsWorkItem>(task => task
                        .Step<Step1>()))))
            {
                var factory = context.Resolve<ITaskFactory>();
                var runner = context.Resolve<ITaskRunner>();

                ITask taskWithSteps = factory.Get<TaskWithSteps>();
                runner.Execute(taskWithSteps);

                ITask taskWithoutSteps = factory.Get<TaskWithoutSteps>();
                runner.Execute(taskWithoutSteps);
            }
        }
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public class TaskWithoutSteps : Task
#pragma warning restore CS0618 // Type or member is obsolete
    {
        public override void StartTask(ITaskExecutionContext context)
        {
            context.Log.Message(Description);
        }

        public override string Description => nameof(TaskWithoutSteps);
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public class TaskWithSteps : Task<TaskThatHasStepsWorkItem>
#pragma warning restore CS0618 // Type or member is obsolete
    {
        public TaskWithSteps(IEnumerable<IStep<TaskThatHasStepsWorkItem>> steps)
            : base(steps)
        {
        }

        public override TaskThatHasStepsWorkItem Start(ITaskExecutionContext context)
        {
            context.Log.Message("START: {0}", Description);

            return new TaskThatHasStepsWorkItem();
        }

        public override void End(ITaskExecutionContext<TaskThatHasStepsWorkItem> context)
        {
            context.Log.Message("END: {0}", Description);
        }

        public override string Description => nameof(TaskWithSteps);
    }

    public class Step1 : Step<TaskThatHasStepsWorkItem>
    {
        public override void Execute(ITaskExecutionContext<TaskThatHasStepsWorkItem> context)
        {
            context.Log.Message(Description);
        }

        public override string Description => nameof(Step1);
    }

    public class TaskThatHasStepsWorkItem
    {
    }
}