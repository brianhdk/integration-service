using System;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using Vertica.Integration.Infrastructure.Email;

namespace Vertica.Integration.Domain.Monitoring
{
    internal class MonitorEmailTemplate : EmailTemplate
    {
        private readonly MonitorEntry[] _entries;
        private readonly MonitorTarget _target;

        public MonitorEmailTemplate(string subject, MonitorEntry[] entries, MonitorTarget target)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));
            if (target == null) throw new ArgumentNullException(nameof(target));

            Subject = subject;
            _entries = entries;
            _target = target;
        }

        public override string Subject { get; }

        public override bool IsHtml => true;

        public override MailPriority? MailPriority => _target.MailPriority;

        public override string GetBody()
        {
            var sb = new StringBuilder();

            foreach (var entry in _entries)
                sb.AppendLine($@"
<tr>
	<td valign=""top"">{entry.DateTime.ToString(CultureInfo.GetCultureInfo("en-US"))}</td>
	<td valign=""top"">{WebUtility.HtmlEncode(entry.Source)}</td>
	<td valign=""top"">{WebUtility.HtmlEncode(entry.Message ?? string.Empty).Replace(Environment.NewLine, "<br />")}</td>
</tr>
");

            return $@"
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

	{sb}
</table>
";
        }
    }
}