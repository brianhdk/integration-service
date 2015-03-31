using System;
using System.IO;
using Vertica.Integration.Infrastructure.Email;
using Vertica.Integration.Infrastructure.Templating;

namespace Vertica.Integration.Domain.Monitoring
{
    internal class MonitorEmailTemplate : EmailTemplate
	{
        private readonly string _subject;
        private readonly MonitorEntry[] _entries;

		public MonitorEmailTemplate(string subject, MonitorEntry[] entries)
		{
		    if (entries == null) throw new ArgumentNullException("entries");

		    _subject = subject;
		    _entries = entries;
		}

	    public override string Subject
		{
			get { return _subject; }
		}

		public override bool IsHtml
		{
			get { return true; }
		}

	    public override string GetBody()
	    {
            using (var stream = new MemoryStream(Resources.EmailTemplate))
            using (var reader = new StreamReader(stream))
            {
                return InMemoryRazorEngine.Execute(
                    reader.ReadToEnd(),
                    _entries,
                    new { },
                    typeof(MonitorEmailTemplate).Assembly);                
            }
	    }
	}
}