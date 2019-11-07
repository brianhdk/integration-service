using Castle.MicroKernel;
using Serilog;

namespace Vertica.Integration.Serilog.Infrastructure
{
    public abstract class Logger
    {
        protected internal abstract ILogger Create(IKernel kernel);
    }
}