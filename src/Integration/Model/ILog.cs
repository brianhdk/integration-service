using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Model
{
    public interface ILog
    {
        void Message(string format, params object[] args);
        void Warning(ITarget target, string format, params object[] args);
        void Error(ITarget target, string format, params object[] args);
    }
}