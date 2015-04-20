using System.Text;

namespace Vertica.Integration.Infrastructure.Email
{
    public class TextBasedEmailTemplate : EmailTemplate
    {
        private readonly string _subject;
        private readonly StringBuilder _text;

        public TextBasedEmailTemplate(string subject)
        {
            _subject = subject;
            _text = new StringBuilder();
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
    }
}