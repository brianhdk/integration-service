using System;
using System.IO;
using System.Threading;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
    /// <summary>
    /// Use the UseTextFileLogger(...) extension method from <see cref="TextFileLoggerExtensions"/> for more control and configuration.
    /// </summary>
    public class TextFileLogger : Logger
    {
        private readonly TextFileLoggerConfiguration _configuration;

        public TextFileLogger(TextFileLoggerConfiguration configuration = null)
        {
            _configuration = configuration ?? new TextFileLoggerConfiguration();
        }

        protected override string Insert(TaskLog log)
        {
            Thread.Sleep(1);

            string fileName = String.Format("{0:yyyyMMddHHmmss-fff}-{1}.txt", log.TimeStamp.LocalDateTime, log.Name);

            File.WriteAllText(fileName, String.Join(Environment.NewLine,
                log.MachineName,
                log.IdentityName,
                log.CommandLine,
                Environment.NewLine,
                Line(log, log.Name)));

            return fileName;
        }

        protected override string Insert(MessageLog log)
        {
            File.AppendAllText(log.TaskLog.Id, Line(log, log.Message));

            return log.TaskLog.Id;
        }

        protected override string Insert(StepLog log)
        {
            File.AppendAllText(log.TaskLog.Id, Line(log, log.Name));

            return log.TaskLog.Id;
        }

        protected override string Insert(ErrorLog log)
        {
            Thread.Sleep(1);

            string fileName = String.Format("ERROR-{0:yyyyMMddHHmmss-fff}-{1}.txt", log.TimeStamp.LocalDateTime, log.Target);

            // TODO: Timestamp

            File.WriteAllText(fileName, String.Join(Environment.NewLine,
                log.MachineName,
                log.IdentityName,
                log.CommandLine,
                log.Message,
                log.FormattedMessage));

            return fileName;
        }

        protected override void Update(TaskLog log)
        {
            File.AppendAllText(log.Id, Line(log, "Task execution time: {0} seconds", log.ExecutionTimeSeconds));
        }

        protected override void Update(StepLog log)
        {
            File.AppendAllText(log.TaskLog.Id, Line(log, "Step execution time: {0} seconds", log.ExecutionTimeSeconds));
        }

        private string Line(LogEntry log, string text, params object[] args)
        {
            return String.Concat(Environment.NewLine, String.Format("[{0:HH:mm:ss}] {1}",
                log.TimeStamp,
                String.Format(text, args)));
        }
    }
}