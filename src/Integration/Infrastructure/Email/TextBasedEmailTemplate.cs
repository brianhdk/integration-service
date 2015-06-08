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

        public TextBasedEmailTemplate(string subject, params object[] args)
        {
            _subject = String.Format(subject, args);
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

        public TextBasedEmailTemplate AddAttachment(Attachment attachment)
        {
            if (attachment == null) throw new ArgumentNullException("attachment");

            _attachments.Add(attachment);

            return this;
        }

        public override string Subject
        {
            get { return _subject; }
        }

        public override bool IsHtml
        {
            get { return false; }
        }

        public override string GetBody()
        {
            return _text.ToString();
        }

        public override IEnumerable<Attachment> Attachments
        {
            get { return _attachments; }
        }
    }
}