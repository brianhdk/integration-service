using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.RavenDB.Infrastructure
{
	public sealed class DefaultConnection : Connection
	{
		public DefaultConnection(ConnectionString connectionString)
			: base(connectionString)
		{
		}
	}
}