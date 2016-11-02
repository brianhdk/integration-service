using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Archiving;

namespace Vertica.Integration.Globase
{
    internal interface IGlobaseConfiguration
    {
        ArchiveOptions ArchiveOptions { get; }
        ConnectionString FtpConnectionString { get; }
    }
}