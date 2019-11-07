namespace Vertica.Integration.Infrastructure.Email
{
	public interface IEmailService
	{
		void Send(EmailTemplate template, params string[] recipients);
	}
}