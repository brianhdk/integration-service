namespace Vertica.Integration.Domain.LiteServer
{
    public interface IRestartable
    {
        bool ShouldRestart(RestartableContext context);
    }
}