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

		public DateTimeOffset DateTime { get; private set; }
	    public string Source { get; private set; }
	    public string Message { get; private set; }
    }
}