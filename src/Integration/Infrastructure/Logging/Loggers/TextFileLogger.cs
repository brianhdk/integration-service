using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
    /// <summary>
    /// Use the UseTextFileLogger(...) extension method from <see cref="TextFileLoggerExtensions"/> for more control and configuration.
    /// </summary>
    public class TextFileLogger : Logger
    {
        private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

        private readonly TextFileLoggerConfiguration _configuration;

        public TextFileLogger(TextFileLoggerConfiguration configuration = null)
        {
            _configuration = configuration ?? new TextFileLoggerConfiguration();
        }

        protected override string Insert(TaskLog log)
        {
            FileInfo filePath = EnsureFilePath(log);

            File.WriteAllText(filePath.FullName, String.Join(Environment.NewLine,
                log.MachineName,
                log.IdentityName,
                log.CommandLine,
                String.Empty,
                "---- BEGIN LOG",
                Line(log, "[{0}]", log.Name)));

            return filePath.Name;
        }

        protected override string Insert(StepLog log)
        {
            FileInfo filePath = EnsureFilePath(log.TaskLog);

            File.AppendAllText(filePath.FullName, Line(log, "[{0}]", log.Name));

            return log.TaskLog.Id;
        }

        protected override string Insert(MessageLog log)
        {
            FileInfo filePath = EnsureFilePath(log.TaskLog);

            string source = ((LogEntry)log.StepLog ?? log.TaskLog).ToString(); 

            File.AppendAllText(filePath.FullName, Line(log, "[{0}] {1}", source, log.Message));

            return log.TaskLog.Id;
        }

        protected override string Insert(ErrorLog log)
        {
            FileInfo filePath = EnsureFilePath(log);

            File.WriteAllText(filePath.FullName, String.Join(Environment.NewLine,
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
                log.FormattedMessage));

            return filePath.Name;
        }

        protected override void Update(TaskLog log)
        {
            FileInfo filePath = EnsureFilePath(log);

            var sb = new StringBuilder();

            if (log.ErrorLog != null)
                sb.Append(ErrorLine(log.ErrorLog, log.Name));

            sb.Append(EndLine(log, log.Name));

            File.AppendAllText(filePath.FullName, sb.ToString());
        }

        protected override void Update(StepLog log)
        {
            FileInfo filePath = EnsureFilePath(log.TaskLog);

            var sb = new StringBuilder();

            if (log.ErrorLog != null)
                sb.Append(ErrorLine(log.ErrorLog, log.Name));

            sb.Append(EndLine(log, log.Name));

            File.AppendAllText(filePath.FullName, sb.ToString());
        }

        private FileInfo EnsureFilePath(TaskLog log)
        {
            return EnsureFilePath(_configuration.GetFilePath(log));
        }

        private FileInfo EnsureFilePath(ErrorLog log)
        {
            return EnsureFilePath(_configuration.GetFilePath(log));
        }

        private FileInfo EnsureFilePath(FileInfo filePath)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");

            DirectoryInfo directory = filePath.Directory;

            if (directory == null)
                throw new InvalidOperationException(
                    String.Format("No directory specified for path '{0}'.", filePath.FullName));

            if (!directory.Exists)
                directory.Create();

            return filePath;
        }

        private string Line(LogEntry log, string text, params object[] args)
        {
            return Line(log.TimeStamp, text, args);
        }

        private string Line(DateTimeOffset timestamp, string text, params object[] args)
        {
            return String.Concat(Environment.NewLine, String.Format("[{0:HH:mm:ss}] {1}", timestamp.LocalDateTime, String.Format(text, args)));
        }

        private string EndLine(LogEntry log, string name)
        {
            return Line(log, "[{0}] Execution time: {1} second(s)", name, log.ExecutionTimeSeconds.GetValueOrDefault().ToString(English));
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