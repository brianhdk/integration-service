using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Archiving;

namespace Vertica.Integration.Perfion
{
	public interface IPerfionConfiguration
	{
		ConnectionString ConnectionString { get; }
		ArchiveOptions ArchiveOptions { get; }
	}
}