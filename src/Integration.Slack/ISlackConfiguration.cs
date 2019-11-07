namespace Vertica.Integration.Slack
{
    public interface ISlackConfiguration
    {
        bool Enabled { get; }

        string BotUserToken { get; }
        string DefaultChannel { get; }
    }
}