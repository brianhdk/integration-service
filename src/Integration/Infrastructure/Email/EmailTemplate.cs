using System.Collections.Generic;
using System.Net.Mail;

namespace Vertica.Integration.Infrastructure.Email
{
	public abstract class EmailTemplate
	{
		public abstract string Subject { get; }
		public abstract bool IsHtml { get; }
	    public abstract string GetBody();

	    public virtual MailPriority? MailPriority
	    {
	        get { return null; }
	    }

	    public virtual IEnumerable<Attachment> Attachments
	    {
	        get { yield break; }
	    }
	}
}