using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Archiving;

namespace Vertica.Integration.Perfion
{
    internal interface IPerfionConfiguration
    {
        ConnectionString ConnectionString { get; }
        ArchiveOptions ArchiveOptions { get; }
        ServiceClientConfiguration ServiceClientConfiguration { get; }
        WebClientConfiguration WebClientConfiguration { get; }
    }
}