using System.Threading.Tasks;

namespace Vertica.Integration.Slack.Bot
{
    public interface ISlackBot
    {
        Task Worker { get; }
    }
}