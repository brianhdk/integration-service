using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
    public class TextFileLoggerConfiguration : IInitializable<IWindsorContainer>
    {
        private Organizer _organizer;

        public TextFileLoggerConfiguration OrganizeSubFoldersBy(Func<BasedOn, Organizer> basedOn)
        {
            _organizer = basedOn(new BasedOn());

            return this;
        }

		internal FileInfo GetFilePath(TaskLog log, string baseDirectory)
        {
            if (log == null) throw new ArgumentNullException("log");

            return Combine(baseDirectory, log.TimeStamp, "{0}", log.Name);
        }

		internal FileInfo GetFilePath(ErrorLog log, string baseDirectory)
        {
            if (log == null) throw new ArgumentNullException("log");

            return Combine(baseDirectory, log.TimeStamp, "{0}-{1}", log.Severity, log.Target);
        }

        private FileInfo Combine(string baseDirectory, DateTimeOffset timestamp, string postFixFormat, params object[] args)
        {
            // Sleep because we use timestamp part of our filename.
            Thread.Sleep(1);

            string fileName = String.Format("{0:yyyyMMddHHmmss-fff}-{1}.txt",
                timestamp.LocalDateTime,
                String.Format(postFixFormat, args));

            string subdirectory = _organizer != null ? _organizer.SubdirectoryName(timestamp) : null;

            return new FileInfo(Path.Combine(baseDirectory, subdirectory ?? String.Empty, fileName));
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

		void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
		{
			container.RegisterInstance(this);
		}
    }
}