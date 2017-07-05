using System;

namespace Vertica.Integration.Domain.Monitoring
{
    public class MonitorEntry
    {
		public MonitorEntry(DateTimeOffset dateTime, string source, string message)
        {
            DateTime = dateTime;
			Source = source;
			Message = message;
        }

		public DateTimeOffset DateTime { get; }
	    public string Source { get; }
	    public string Message { get; }
    }
}