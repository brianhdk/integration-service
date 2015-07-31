using Raven.Client;
using Vertica.Integration.RavenDB.Infrastructure;

namespace Vertica.Integration.RavenDB
{
	public interface IRavenDBFactory : IRavenDBFactory<DefaultConnection>
	{
	}

	public interface IRavenDBFactory<TConnection>
		where TConnection : Connection
	{
		IDocumentStore DocumentStore { get; }
	}
}