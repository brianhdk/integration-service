using System;
using System.Globalization;
using System.IO;
using System.Text;
using Vertica.Utilities_v4.Extensions.StringExt;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
	internal class TextFileLogger : Logger
    {
        private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

		private readonly string _baseDirectory;
		private readonly TextFileLoggerConfiguration _configuration;

        public TextFileLogger(IRuntimeSettings settings, TextFileLoggerConfiguration configuration)
        {
	        _baseDirectory = settings["TextLogger.BaseDirectory"].NullIfEmpty() ?? "Data\\Logs";
	        _configuration = configuration;
        }

		protected override string Insert(TaskLog log)
        {
            FileInfo filePath = EnsureFilePath(log);

            File.WriteAllText(filePath.FullName, string.Join(Environment.NewLine,
                log.MachineName,
                log.IdentityName,
                log.CommandLine,
				string.Empty,
                "---- BEGIN LOG",
                Line(log)));

            return filePath.Name;
        }

        protected override string Insert(StepLog log)
        {
            FileInfo filePath = EnsureFilePath(log.TaskLog);

            File.AppendAllText(filePath.FullName, Line(log));

            return log.TaskLog.Id;
        }

        protected override string Insert(MessageLog log)
        {
            FileInfo filePath = EnsureFilePath(log.TaskLog);

            File.AppendAllText(filePath.FullName, Line(log, "{0}", log.Message));

            return log.TaskLog.Id;
        }

        protected override string Insert(ErrorLog log)
        {
            FileInfo filePath = EnsureFilePath(log);

            File.WriteAllText(filePath.FullName, string.Join(Environment.NewLine,
                log.MachineName,
                log.IdentityName,
                log.CommandLine,
                log.Severity,
                log.Target,
                log.TimeStamp,
				string.Empty,
                "---- BEGIN LOG",
				string.Empty,
                log.Message,
				string.Empty,
                log.FormattedMessage));

            return filePath.Name;
        }

        protected override void Update(TaskLog log)
        {
            FileInfo filePath = EnsureFilePath(log);

            var sb = new StringBuilder();

            if (log.ErrorLog != null)
                sb.Append(ErrorLine(log.ErrorLog, log.Name));

            sb.Append(EndLine(log));

            File.AppendAllText(filePath.FullName, sb.ToString());
        }

        protected override void Update(StepLog log)
        {
            FileInfo filePath = EnsureFilePath(log.TaskLog);

            var sb = new StringBuilder();

            if (log.ErrorLog != null)
                sb.Append(ErrorLine(log.ErrorLog, log.Name));

            sb.Append(EndLine(log));

            File.AppendAllText(filePath.FullName, sb.ToString());
        }

        private FileInfo EnsureFilePath(TaskLog log)
        {
            return EnsureFilePath(_configuration.GetFilePath(log, _baseDirectory));
        }

        private FileInfo EnsureFilePath(ErrorLog log)
        {
            return EnsureFilePath(_configuration.GetFilePath(log, _baseDirectory));
        }

        private FileInfo EnsureFilePath(FileInfo filePath)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));

            DirectoryInfo directory = filePath.Directory;

            if (directory == null)
                throw new InvalidOperationException(
	                $"No directory specified for path '{filePath.FullName}'.");

            if (!directory.Exists)
                directory.Create();

            return filePath;
        }

        private string Line(LogEntry log, string text = null, params object[] args)
        {
            if (!string.IsNullOrWhiteSpace(text))
                text = string.Concat(" ", string.Format(text, args));

            return Line(log.TimeStamp, $"[{log}]{text}");
        }

        private string Line(DateTimeOffset timestamp, string text, params object[] args)
        {
            return string.Concat(Environment.NewLine,
	            $"[{timestamp.LocalDateTime:HH:mm:ss}] {string.Format(text, args)}");
        }

        private string EndLine(LogEntry log)
        {
            return Line(log, "Execution time: {0} second(s)", log.ExecutionTimeSeconds.GetValueOrDefault().ToString(English));
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