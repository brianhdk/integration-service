using System.Collections.Generic;

namespace Vertica.Integration.Infrastructure.Email
{
	public interface IEmailService
	{
		void Send(EmailTemplate template, IEnumerable<string> recipients);
	}
}