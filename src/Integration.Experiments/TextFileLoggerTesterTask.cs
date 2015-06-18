using System;
using System.Collections.Generic;
using System.IO;
using Castle.Core.Internal;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Logging.Loggers;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class TextFileLoggerTesterTask : Task
    {
        private readonly ITaskFactory _factory;
        private readonly ITaskRunner _runner;

        public TextFileLoggerTesterTask(ITaskFactory factory, ITaskRunner runner)
        {
            _factory = factory;
            _runner = runner;
        }

        public override string Description
        {
            get { return "Tests TextFileLogger"; }
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            //throw new InvalidOperationException(@"This can't be done!");
            context.Log.Message("Message from First Task");
            context.Log.Warning(Target.Service, "Warning from First Task");
            //context.Log.Error(Target.Custom("Custom"), "Error from First Task");

            // TODO: Test with Exception thrown

            _runner.Execute(_factory.Get<TextFileLoggerTesterWithStepsTask>());
        }
    }

    public class TextFileLoggerTesterWithStepsTask : Task<TextFileLoggerTesterWorkItem>
    {
        public TextFileLoggerTesterWithStepsTask(IEnumerable<IStep<TextFileLoggerTesterWorkItem>> steps)
            : base(steps)
        {
        }

        public override TextFileLoggerTesterWorkItem Start(ITaskExecutionContext context)
        {
            context.Log.Message("Message from Second Task - Start");
            //context.Log.Warning(Target.Service, "Warning from Second Task - Start");
            //context.Log.Error(Target.Custom("Custom"), "Error from Second Task - Start");

            return new TextFileLoggerTesterWorkItem();
        }

        public override void End(TextFileLoggerTesterWorkItem workItem, ITaskExecutionContext context)
        {
            context.Log.Message("Message from Second Task - End");
            //context.Log.Warning(Target.Service, "Warning from Second Task - End");
            //context.Log.Error(Target.Custom("Custom"), "Error from Second Task - End");
        }

        public override string Description
        {
            get { return "Tests TextFileLogger with Steps"; }
        }
    }

    public class TextFileLoggerTesterStep : Step<TextFileLoggerTesterWorkItem>
    {
        public override string Description
        {
            get { return "Tester with Steps"; }
        }

        public override void Execute(TextFileLoggerTesterWorkItem workItem, ITaskExecutionContext context)
        {
            context.Log.Message("Message from Step");
            //context.Log.Warning(Target.Service, "Warning from Step");
            //context.Log.Error(Target.Custom("Custom"), "Error from Step");

            // TODO: Test with Exception thrown
            //throw new InvalidOperationException("Blaah");
        }
    }

    public class TextFileLoggerTesterWorkItem
    {
    }

    public static class TestTextFileLoggerTesterExtensions
    {
        public static ApplicationConfiguration TestTextFileLogger(this ApplicationConfiguration application)
        {
            Directory.EnumerateFiles(@"D:\github\integration-service\src\Integration.Console\bin\Debug\Logs").ForEach(File.Delete);

            return application
                .Logging(logger => logger
                    .UseTextFileLogger(/*textFile => textFile
                        .ToLocation(new DirectoryInfo(@"c:\tmp"))
                        .OrganizeSubFoldersBy(x => x.Custom(new CustomOrganizer()))*/))
                .Tasks(tasks => tasks
                    .Clear()
                    .Task<TextFileLoggerTesterTask>()
                    .Task<TextFileLoggerTesterWithStepsTask, TextFileLoggerTesterWorkItem>(task => task
                        .Step<TextFileLoggerTesterStep>()));
        }

        public class CustomOrganizer : TextFileLoggerConfiguration.Organizer
        {
            public override string SubdirectoryName(DateTimeOffset date)
            {
                return date.ToString("yyyy");
            }
        }
    }
}