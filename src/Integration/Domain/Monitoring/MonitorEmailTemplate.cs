using System;
using System.IO;
using Vertica.Integration.Infrastructure.Email;
using Vertica.Integration.Infrastructure.Templating;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Domain.Monitoring
{
	public class MonitorEmailTemplate : EmailTemplate
	{
	    private readonly Range<DateTimeOffset> _checkRange;
	    private readonly MonitorEntry[] _entries;

		public MonitorEmailTemplate(Range<DateTimeOffset> checkRange, MonitorEntry[] entries)
		{
		    if (checkRange == null) throw new ArgumentNullException("checkRange");
		    if (entries == null) throw new ArgumentNullException("entries");

		    _checkRange = checkRange;
		    _entries = entries;
		}

	    public override string Subject
		{
			get { return String.Format("Integration Service: Monitoring ({0})", _checkRange); }
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