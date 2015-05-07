using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Model
{
    public interface ILog
    {
        void Message(string format, params object[] args);
        void Warning(Target target, string format, params object[] args);
        void Error(Target target, string format, params object[] args);
    }
}