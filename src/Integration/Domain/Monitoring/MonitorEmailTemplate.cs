using System;
using System.Collections.Generic;
using Vertica.Integration.Infrastructure.Email;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Domain.Monitoring
{
	public class MonitorEmailTemplate : EmailTemplate
	{
	    private readonly Range<DateTimeOffset> _checkRange;
	    private readonly IEnumerable<MonitorEntry> _entries;

		public MonitorEmailTemplate(Range<DateTimeOffset> checkRange, IEnumerable<MonitorEntry> entries)
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

		protected override string Template
		{
			get { return @"
<h1>Integration Service: Monitoring</h1>
<p>
	<b>One or more errors/warnings occurred, see table below for further details.</b>
</p>

<table border=""1"" cellspacing=""5"" cellpadding=""5"">
	<tr>
		<td><b>DateTime (UTC)</b></td>
		<td><b>Source</b></td>
		<td><b>Description</b></td>
	</tr>

	#foreach ($entry in $entries)

	<tr>
		<td valign=""top"">$entry.DateTime</td>
		<td valign=""top"">$entry.Source</td>
		<td valign=""top"">$helper.LinesToHtml($entry.Message)</td>
	</tr>

	#end

</table>
"; 
			}
		}

		protected override IEnumerable<KeyValuePair<string, object>> Parameters
		{
			get
			{
				yield return new KeyValuePair<string, object>("entries", _entries);
			}
		}
	}
}