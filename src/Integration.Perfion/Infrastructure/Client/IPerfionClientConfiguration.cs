using Vertica.Integration.Infrastructure.Archiving;

namespace Vertica.Integration.Perfion.Infrastructure.Client
{
    internal interface IPerfionClientConfiguration
    {
        ArchiveOptions ArchiveOptions { get; }
    }
}