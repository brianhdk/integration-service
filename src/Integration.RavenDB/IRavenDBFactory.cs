using System;
using Raven.Client;
using Vertica.Integration.RavenDB.Infrastructure;

namespace Vertica.Integration.RavenDB
{
	public interface IRavenDbFactory : IRavenDbFactory<DefaultConnection>
	{
	}

	public interface IRavenDbFactory<TConnection> : IDisposable
		where TConnection : Connection
	{
		IDocumentStore DocumentStore { get; }
	}
}