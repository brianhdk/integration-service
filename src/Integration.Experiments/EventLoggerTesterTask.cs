using System.Collections.Generic;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Logging.Loggers;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class EventLoggerTesterTask : Task
    {
        private readonly ITaskFactory _factory;
        private readonly ITaskRunner _runner;

        public EventLoggerTesterTask(ITaskFactory factory, ITaskRunner runner)
        {
            _factory = factory;
            _runner = runner;
        }

        public override string Description
        {
            get { return "Tests EventLogger"; }
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            //throw new InvalidOperationException(@"This can't be done!");
            //context.Log.Message("Message from First Task");
            //context.Log.Warning(Target.Service, "Warning from First Task");
            //context.Log.Error(Target.Custom("Custom"), "Error from First Task");

            //// TODO: Test with Exception thrown

            _runner.Execute(_factory.Get<EventLoggerTesterWithStepsTask>());
        }
    }

    public class EventLoggerTesterWithStepsTask : Task<EventLoggerTesterWorkItem>
    {
        public EventLoggerTesterWithStepsTask(IEnumerable<IStep<EventLoggerTesterWorkItem>> steps)
            : base(steps)
        {
        }

        public override EventLoggerTesterWorkItem Start(ITaskExecutionContext context)
        {
            context.Log.Message("Message from Second Task - Start");
            context.Log.Warning(Target.Service, "Warning from Second Task - Start");
            //context.Log.Error(Target.Custom("Custom"), "Error from Second Task - Start");

            return new EventLoggerTesterWorkItem();
        }

        public override void End(EventLoggerTesterWorkItem workItem, ITaskExecutionContext context)
        {
            context.Log.Message("Message from Second Task - End");
            context.Log.Warning(Target.Service, "Warning from Second Task - End");
            //context.Log.Error(Target.Custom("Custom"), "Error from Second Task - End");
        }

        public override string Description
        {
            get { return "Tests TextFileLogger with Steps"; }
        }
    }

    public class EventLoggerTesterStep : Step<EventLoggerTesterWorkItem>
    {
        public override string Description
        {
            get { return "Tester with Steps"; }
        }

        public override void Execute(EventLoggerTesterWorkItem workItem, ITaskExecutionContext context)
        {
            context.Log.Message("Message from Step");
            //context.Log.Warning(Target.Service, "Warning from Step");
            //context.Log.Error(Target.Custom("Custom"), "Error from Step");

            // TODO: Test with Exception thrown
            //throw new InvalidOperationException("Blaah");
        }
    }

    public class EventLoggerTesterWorkItem
    {
    }

    public static class TestEventLoggerTesterExtensions
    {
        public static ApplicationConfiguration TestEventLogger(this ApplicationConfiguration application)
        {
            return application
                .Fast()
                .Logging(logger => logger.EventLogger())
                .Tasks(tasks => tasks
                    .Clear()
                    .Task<EventLoggerTesterTask>()
                    .Task<EventLoggerTesterWithStepsTask, EventLoggerTesterWorkItem>(task => task
                        .Step<EventLoggerTesterStep>()));
        }
    }
}