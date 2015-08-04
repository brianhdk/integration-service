using Raven.Client;
using Vertica.Integration.RavenDB.Infrastructure;

namespace Vertica.Integration.RavenDB
{
	public interface IRavenDbFactory : IRavenDbFactory<DefaultConnection>
	{
	}

	public interface IRavenDbFactory<TConnection>
		where TConnection : Connection
	{
		IDocumentStore DocumentStore { get; }
	}
}