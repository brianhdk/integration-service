using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;

namespace Experiments.Console.Tasks.WithSteps
{
    public static class Demo
    {
        public static void Run()
        {
            using (var context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Disable()))
                .Services(services => services
                    .Advanced(advanced => advanced
                        // We'll override where to read RuntimeSettings from - just for demo purposes
                        // It's recommended to configure these in the app.config instead - which is the default behaviour
                        .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings()
                            .Set("SomeAppSetting", "This is a value read from AppSettings."))))
                .Tasks(tasks => tasks
                    // A tash that has steps has to be explicitly registred in order to specify the sequence of the steps.
                    .Task<TaskThatHasSteps, TaskThatHasStepsWorkItem>(task => task
                        .Step<Step1>()
                        .Step<Step2>()
                        .Step<Step3>()
                        .Step<Step4>()
                        .Step<Step5>()))))
            {
                var factory = context.Resolve<ITaskFactory>();
                var runner = context.Resolve<ITaskRunner>();

                ITask task = factory.Get<TaskThatHasSteps>();
                runner.Execute(task, new Vertica.Integration.Model.Arguments("Argument 1"));
            }
        }
    }

    public class TaskThatHasSteps : IntegrationTask<TaskThatHasStepsWorkItem>
    {
        private readonly IRuntimeSettings _settings;

        public TaskThatHasSteps(IRuntimeSettings settings, IEnumerable<IStep<TaskThatHasStepsWorkItem>> steps)
            : base(steps)
        {
            _settings = settings;
        }

        public override TaskThatHasStepsWorkItem Start(ITaskExecutionContext context)
        {
            return new TaskThatHasStepsWorkItem(_settings["SomeAppSetting"]);
        }

        public override void End(ITaskExecutionContext<TaskThatHasStepsWorkItem> context)
        {
            context.Log.Message($"Task ended, these steps ran their Execute()-method: [{string.Join(", ", context.WorkItem)}]");
        }

        public override string Description => "Illustrates an example of a Task that has steps.";
    }

    public class Step1 : Step<TaskThatHasStepsWorkItem>
    {
        public override void Execute(ITaskExecutionContext<TaskThatHasStepsWorkItem> context)
        {
            context.WorkItem.Register(this);

            context.Log.Message($"Hi from Step1 - {context.WorkItem.AppSettingsValue}");
        }

        public override string Description => nameof(Step1);
    }

    public class Step2 : Step<TaskThatHasStepsWorkItem>
    {
        public override Execution ContinueWith(ITaskExecutionContext<TaskThatHasStepsWorkItem> context)
        {
            context.Log.Warning(Target.All, "Step2 will StepOver.");

            return Execution.StepOver;
        }

        public override void Execute(ITaskExecutionContext<TaskThatHasStepsWorkItem> context)
        {
            context.WorkItem.Register(this);

            context.Log.Message("Hi from Step2 - if this message appears, something is wrong - as we're supposed to StepOver this Step (see ContinueWith(...) implementation");
        }

        public override string Description => nameof(Step2);
    }

    public class Step3 : Step<TaskThatHasStepsWorkItem>
    {
        public override Execution ContinueWith(ITaskExecutionContext<TaskThatHasStepsWorkItem> context)
        {
            ExtremelyExpensiveObject expensiveObject = context.TypedBag("SomeExpensiveObject", new ExtremelyExpensiveObject());

            if (expensiveObject.Data.Length > 0)
                return Execution.Execute;

            return Execution.StepOut;
        }

        public override void Execute(ITaskExecutionContext<TaskThatHasStepsWorkItem> context)
        {
            context.WorkItem.Register(this);

            ExtremelyExpensiveObject expensiveObject = context.TypedBag<ExtremelyExpensiveObject>("SomeExpensiveObject");

            context.Log.Message(Encoding.Default.GetString(expensiveObject.Data));
        }

        public override string Description => nameof(Step3);

        private class ExtremelyExpensiveObject
        {
            private static int _ctorCount;

            public ExtremelyExpensiveObject()
            {
                if (Interlocked.Increment(ref _ctorCount) > 1)
                    throw new InvalidOperationException("Whoa - creating this object is expensive! Should only happen once!");

                Data = Encoding.Default.GetBytes("Hi from Step3");
            }

            public byte[] Data { get; }
        }
    }

    public class Step4 : Step<TaskThatHasStepsWorkItem>
    {
        public override Execution ContinueWith(ITaskExecutionContext<TaskThatHasStepsWorkItem> context)
        {
            context.Log.Error(Target.All, "Step4 will StepOut - exiting the Task flow.");

            return Execution.StepOut;
        }

        public override void Execute(ITaskExecutionContext<TaskThatHasStepsWorkItem> context)
        {
            context.WorkItem.Register(this);

            context.Log.Message("Hi from Step4 - if this message appears, something is wrong - as we're supposed to StepOut before this Step (see ContinueWith(...) implementation");
        }

        public override string Description => nameof(Step4);
    }

    public class Step5 : Step<TaskThatHasStepsWorkItem>
    {
        public override void Execute(ITaskExecutionContext<TaskThatHasStepsWorkItem> context)
        {
            context.WorkItem.Register(this);

            context.Log.Message("Hi from Step5 - if this message appears, something is wrong - as we're supposed to StepOut from the previous Step (see ContinueWith(...) implementation on Step4");
        }

        public override string Description => nameof(Step5);
    }

    public class TaskThatHasStepsWorkItem : IEnumerable<string>
    {
        private readonly List<string> _steps;

        public TaskThatHasStepsWorkItem(string appSettingsValue)
        {
            AppSettingsValue = appSettingsValue;

            _steps = new List<string>();
        }

        public string AppSettingsValue { get; }

        public void Register(IStep<TaskThatHasStepsWorkItem> step)
        {
            if (step == null) throw new ArgumentNullException(nameof(step));

            _steps.Add(step.Name());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _steps.GetEnumerator();
        }
    }
}