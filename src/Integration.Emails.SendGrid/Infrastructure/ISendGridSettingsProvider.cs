namespace Vertica.Integration.Emails.SendGrid.Infrastructure
{
    public interface ISendGridSettingsProvider
    {
        SendGridSettings Get();
    }
}