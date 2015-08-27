using System;
using System.IO;
using System.Net.Mail;
using System.Web;
using Vertica.Integration.Infrastructure.Email;
using Vertica.Integration.Infrastructure.Templating;

namespace Vertica.Integration.Domain.Monitoring
{
    internal class MonitorEmailTemplate : EmailTemplate
	{
        private readonly string _subject;
        private readonly MonitorEntry[] _entries;
        private readonly MonitorTarget _target;

        public MonitorEmailTemplate(string subject, MonitorEntry[] entries, MonitorTarget target)
		{
		    if (entries == null) throw new ArgumentNullException("entries");
		    if (target == null) throw new ArgumentNullException("target");

		    _subject = subject;
		    _entries = entries;
            _target = target;
		}

	    protected internal override string Subject
		{
			get { return _subject; }
		}

		protected internal override bool IsHtml
		{
			get { return true; }
		}

		protected internal override string GetBody()
	    {
            using (var stream = new MemoryStream(Resources.EmailTemplate))
            using (var reader = new StreamReader(stream))
            {
                return InMemoryRazorEngine.Execute(
                    reader.ReadToEnd(),
                    _entries,
                    new { },
                    typeof(MonitorEmailTemplate).Assembly,
                    typeof(HttpServerUtility).Assembly);
            }
	    }

		protected internal override MailPriority? MailPriority
        {
            get { return _target.MailPriority; }
        }
	}
}