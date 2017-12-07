namespace Vertica.Integration.Emails.Mandrill.Infrastructure
{
    public interface IMandrillSettingsProvider
    {
        MandrillSettings Get();
    }
}