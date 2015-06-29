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
        private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

        protected override string Insert(TaskLog log)
        {
            return null;
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
                "Integration Service", 
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
                sb.AppendLine(String.Format("[{0}]", entry));

                // if TaskLog || StepLog && .Error
            }

            EventLog.WriteEntry(
                "Integration Service",
                sb.ToString(),
                EventLogEntryType.Information,
                GenerateEventId());
        }

        protected override void Update(StepLog log)
        {
        }

        // http://stackoverflow.com/questions/951702/unique-eventid-generation
        public static int GenerateEventId()
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
    }
}