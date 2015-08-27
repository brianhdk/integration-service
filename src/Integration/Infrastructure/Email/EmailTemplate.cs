using System.Collections.Generic;
using System.Net.Mail;

namespace Vertica.Integration.Infrastructure.Email
{
	public abstract class EmailTemplate
	{
		protected internal abstract string Subject { get; }
		protected internal abstract bool IsHtml { get; }
	    protected internal abstract string GetBody();

		protected internal virtual MailPriority? MailPriority
	    {
	        get { return null; }
	    }

	    protected internal virtual IEnumerable<Attachment> Attachments
	    {
	        get { yield break; }
	    }
	}
}