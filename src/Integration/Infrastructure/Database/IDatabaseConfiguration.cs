using System;
using Castle.MicroKernel;

namespace Vertica.Integration.Infrastructure.Database
{
    public interface IDatabaseConfiguration
    {
        bool IntegrationDbDisabled { get; }
        
        Func<IKernel, string> IntegrationDbTablePrefix { get; }
    }
}