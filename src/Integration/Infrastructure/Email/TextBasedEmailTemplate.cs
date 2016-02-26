using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace Vertica.Integration.Infrastructure.Email
{
    public class TextBasedEmailTemplate : EmailTemplate
    {
        private readonly string _subject;
        private readonly StringBuilder _text;
        private readonly List<Attachment> _attachments;
		private MailPriority? _priority;

        public TextBasedEmailTemplate(string subject, params object[] args)
        {
            _subject = string.Format(subject, args);
            _text = new StringBuilder();
            _attachments = new List<Attachment>();
        }

        public TextBasedEmailTemplate Write(string format, params object[] args)
        {
            _text.AppendFormat(format, args);
            return this;
        }

        public TextBasedEmailTemplate WriteLine(string format, params object[] args)
        {
            Write(format, args);
            _text.AppendLine();

            return this;
        }

	    public TextBasedEmailTemplate Priority(MailPriority priority)
	    {
		    _priority = priority;

		    return this;
	    }

        public TextBasedEmailTemplate AddAttachment(Attachment attachment)
        {
            if (attachment == null) throw new ArgumentNullException(nameof(attachment));

            _attachments.Add(attachment);

            return this;
        }

		protected internal override string Subject => _subject;

	    protected internal override bool IsHtml => false;

	    protected internal override string GetBody()
        {
            return _text.ToString();
        }

		protected internal override MailPriority? MailPriority => _priority;

	    protected internal override IEnumerable<Attachment> Attachments => _attachments;
    }
}