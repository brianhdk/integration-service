using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
    public class TextFileLoggerConfiguration
    {
        private DirectoryInfo _location;
        private Organizer _organizer;

        public TextFileLoggerConfiguration()
        {
            _location = new DirectoryInfo("Logs");
        }

        public TextFileLoggerConfiguration ToLocation(DirectoryInfo location)
        {
            if (location == null) throw new ArgumentNullException("location");

            _location = location;

            return this;
        }

        public TextFileLoggerConfiguration OrganizeSubFoldersBy(Func<BasedOn, Organizer> basedOn)
        {
            _organizer = basedOn(new BasedOn());

            return this;
        }

        internal FileInfo GetFilePath(TaskLog log)
        {
            if (log == null) throw new ArgumentNullException("log");

            return Combine(log.TimeStamp, "{0}", log.Name);
        }

        internal FileInfo GetFilePath(ErrorLog log)
        {
            if (log == null) throw new ArgumentNullException("log");

            return Combine(log.TimeStamp, "{0}-{1}", log.Severity, log.Target);
        }

        private FileInfo Combine(DateTimeOffset timestamp, string postFixFormat, params object[] args)
        {
            // Sleep because we use timestamp part of our filename.
            Thread.Sleep(1);

            string fileName = String.Format("{0:yyyyMMddHHmmss-fff}-{1}.txt",
                timestamp.LocalDateTime,
                String.Format(postFixFormat, args));

            string subdirectory = _organizer != null ? _organizer.SubdirectoryName(timestamp) : null;

            return new FileInfo(Path.Combine(_location.FullName, subdirectory ?? String.Empty, fileName));
        }

        public class BasedOn
        {
            public Organizer Daily { get { return new DailyOrganizer(); } }
            public Organizer Weekly { get { return new WeeklyOrganizer(); } }
            public Organizer Monthly { get { return new MonthlyOrganizer(); } }

            public Organizer Custom(Organizer organizer)
            {
                if (organizer == null) throw new ArgumentNullException("organizer");

                return organizer;
            }
        }

        public abstract class Organizer
        {
            public abstract string SubdirectoryName(DateTimeOffset date);
        }

        public class DailyOrganizer : Organizer
        {
            public override string SubdirectoryName(DateTimeOffset date)
            {
                return date.LocalDateTime.ToString("yyyyMMdd");
            }
        }

        public class WeeklyOrganizer : Organizer
        {
            public override string SubdirectoryName(DateTimeOffset date)
            {
                int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                    date.LocalDateTime,
                    CalendarWeekRule.FirstFourDayWeek,
                    DayOfWeek.Monday);

                return String.Format("{0:yyyy}-{1:00}", date.LocalDateTime, weekNumber);
            }
        }

        public class MonthlyOrganizer : Organizer
        {
            public override string SubdirectoryName(DateTimeOffset date)
            {
                return date.LocalDateTime.ToString("yyyyMM");
            }
        }
    }
}