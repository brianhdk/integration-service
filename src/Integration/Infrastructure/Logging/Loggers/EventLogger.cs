using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
    public class EventLogger : Logger
    {
        private const string SourceName = "Integration Service";
        private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

        protected override string Insert(TaskLog log)
        {
            return GenerateEventId().ToString();
        }

        protected override string Insert(MessageLog log)
        {
            return null;
        }

        protected override string Insert(StepLog log)
        {
            return null;
        }

        protected override string Insert(ErrorLog log)
        {
            int id = GenerateEventId();

            string message = String.Join(Environment.NewLine,
                log.MachineName,
                log.IdentityName,
                log.CommandLine,
                log.Severity,
                log.Target,
                log.TimeStamp,
                String.Empty,
                "---- BEGIN LOG",
                String.Empty,
                log.Message,
                String.Empty,
                log.FormattedMessage);

            EventLog.WriteEntry(
                SourceName, 
                message, 
                log.Severity == Severity.Error ? EventLogEntryType.Error : EventLogEntryType.Warning, 
                id);

            return id.ToString();
        }

        protected override void Update(TaskLog log)
        {
            var sb = new StringBuilder();

            sb.AppendLine(String.Join(Environment.NewLine,
                log.MachineName,
                log.IdentityName,
                log.CommandLine,
                String.Empty,
                "---- BEGIN LOG"));

            IEnumerable<LogEntry> entries = new LogEntry[] { log }
                .Concat(log.Messages)
                .Concat(log.Steps)
                .Concat(log.Steps.SelectMany(s => s.Messages))
                .OrderBy(x => x.TimeStamp);

            foreach (LogEntry entry in entries)
            {
                var messageLog = entry as MessageLog;

                sb.Append(Line(entry, messageLog != null ? messageLog.Message : ExecutionTime(entry)));

                ErrorLog error = CheckGetError(entry);

                if (error != null)
                    sb.Append(ErrorLine(error, entry.ToString()));
            }

            EventLog.WriteEntry(
                SourceName,
                sb.ToString(),
                EventLogEntryType.Information,
                Int32.Parse(log.Id));
        }

        protected override void Update(StepLog log)
        {
        }

        // http://stackoverflow.com/questions/951702/unique-eventid-generation
        private static int GenerateEventId()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Environment.StackTrace);

            StackTrace trace = new StackTrace();
            
            foreach (StackFrame frame in trace.GetFrames() ?? new StackFrame[0])
            {
                sb.Append(frame.GetILOffset());
                sb.Append(",");
            }

            return sb.ToString().GetHashCode() & 0xFFFF;
        }

        private ErrorLog CheckGetError(LogEntry log)
        {
            var reference = log as IReferenceErrorLog;

            return reference != null ? reference.ErrorLog : null;
        }

        private string Line(LogEntry log, string text = null, params object[] args)
        {
            if (!String.IsNullOrWhiteSpace(text))
                text = String.Concat(" ", String.Format(text, args));

            return Line(log.TimeStamp, String.Format("[{0}]{1}", log, text));
        }

        private string Line(DateTimeOffset timestamp, string text, params object[] args)
        {
            return String.Concat(Environment.NewLine, String.Format("[{0:HH:mm:ss}] {1}", timestamp.LocalDateTime, String.Format(text, args)));
        }

        private string ExecutionTime(LogEntry log)
        {
            return String.Format("(Execution time: {0} second(s))", log.ExecutionTimeSeconds.GetValueOrDefault().ToString(English));
        }

        private string ErrorLine(ErrorLog error, string name)
        {
            return Line(error.TimeStamp, "[{0}] [{1}]: {2} (ID: {3})",
                name,
                error.Severity,
                error.Message,
                error.Id);
        }
    }
}