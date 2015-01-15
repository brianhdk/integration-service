using System;
using NHibernate;
using Vertica.Integration.Infrastructure.Database.NHibernate;
using Vertica.Integration.Properties;

namespace Vertica.Integration.Infrastructure.Logging
{
	public class Logger : ILogger
	{
		private readonly Lazy<ISessionFactoryProvider> _provider;
	    private readonly ISettings _settings;

		public Logger(Lazy<ISessionFactoryProvider> provider, ISettings settings)
		{
		    _provider = provider;
		    _settings = settings;
		}

	    public ErrorLog LogError(Target target, string message, params object[] args)
	    {
            return Log(new ErrorLog(Severity.Error, String.Format(message, args), target));
	    }

	    public ErrorLog LogError(Exception exception, Target target = Target.Service)
	    {
	        if (exception == null) throw new ArgumentNullException("exception");

            return Log(new ErrorLog(exception, target));
	    }

	    public ErrorLog LogWarning(Target target, string message, params object[] args)
	    {
	        return Log(new ErrorLog(Severity.Warning, String.Format(message, args), target));
	    }

	    public void LogEntry(LogEntry logEntry)
		{
			if (logEntry == null) throw new ArgumentNullException("logEntry");

		    if (_settings.DisableDatabaseLog)
		        return;

			using (IStatelessSession session = _provider.Value.SessionFactory.OpenStatelessSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				if (logEntry.Id == 0)
				{
				    session.Insert(logEntry);
				}
				else
				{
				    session.Update(logEntry);
				}

				transaction.Commit();
			}
		}

	    private ErrorLog Log(ErrorLog errorLog)
	    {
	        if (_settings.DisableDatabaseLog)
	            return null;

	        using (IStatelessSession session = _provider.Value.SessionFactory.OpenStatelessSession())
	        using (ITransaction transaction = session.BeginTransaction())
	        {
	            session.Insert(errorLog);

	            transaction.Commit();

	            return errorLog;
	        }
	    }
	}
}