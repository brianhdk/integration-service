using System.Collections.Generic;
using System.Net.Mail;
using Vertica.Integration.Infrastructure.Email;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class EmailWithAttachmentTesterTask : Task
    {
        private readonly IEmailService _emailService;

        public EmailWithAttachmentTesterTask(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public override void StartTask(ILog log, params string[] arguments)
        {
            _emailService.Send(new EmailWithAttachment(), new[] {"bhk@vertica.dk"});
        }

        public override string Description
        {
            get { return "TBD"; }
        }

        public class EmailWithAttachment : EmailTemplate
        {
            public override bool IsHtml
            {
                get { return true; }
            }

            public override string GetBody()
            {
                return "Text";
            }

            public override string Subject
            {
                get { return "Subject"; }
            }

            public override IEnumerable<Attachment> Attachments
            {
                get { yield return Attachment.CreateAttachmentFromString("Some", "data.csv", null, null); }
            }
        }
    }
}