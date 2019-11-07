using Serilog;
using Vertica.Integration.Serilog.Infrastructure;

namespace Vertica.Integration.Serilog
{
    public interface ILoggerFactory<TLogger>
        where TLogger : Logger
    {
        ILogger Logger { get; }
    }

    public interface ILoggerFactory : ILoggerFactory<DefaultLogger>
    {
    }
}