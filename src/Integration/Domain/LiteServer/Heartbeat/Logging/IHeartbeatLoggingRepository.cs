namespace Vertica.Integration.Domain.LiteServer.Heartbeat.Logging
{
    public interface IHeartbeatLoggingRepository
    {
        void Insert(HeartbeatLogEntry entry);
    }
}