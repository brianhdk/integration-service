using System;
using System.Collections.Generic;
using NHibernate;
using Vertica.Integration.Infrastructure.Database.NHibernate;

namespace Vertica.Integration.Infrastructure.Logging
{
	public class Logger : ILogger
	{
		private readonly Lazy<ISessionFactoryProvider> _provider;

	    private readonly object _dummy = new object();
	    private readonly Stack<object> _disablers;

		public Logger(Lazy<ISessionFactoryProvider> provider)
		{
		    _provider = provider;

		    _disablers = new Stack<object>();
		}

	    public ErrorLog LogError(Target target, string message, params object[] args)
	    {
            return Log(new ErrorLog(Severity.Error, String.Format(message, args), target));
	    }

	    public ErrorLog LogError(Exception exception, Target target = null)
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

		    if (LoggingDisabled)
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

	    private bool LoggingDisabled
	    {
	        get { return _disablers.Count > 0; }
	    }

	    public IDisposable Disable()
	    {
	        _disablers.Push(_dummy);
	        return new Disabler(() => _disablers.Pop());
	    }

	    private class Disabler : IDisposable
	    {
	        private readonly Action _disposed;

	        public Disabler(Action disposed)
	        {
	            _disposed = disposed;
	        }

	        public void Dispose()
	        {
	            _disposed();
	        }
	    }

	    private ErrorLog Log(ErrorLog errorLog)
	    {
	        if (LoggingDisabled)
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