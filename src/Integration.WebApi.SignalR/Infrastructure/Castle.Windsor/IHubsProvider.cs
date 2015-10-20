using System;

namespace Vertica.Integration.WebApi.SignalR.Infrastructure.Castle.Windsor
{
	internal interface IHubsProvider
	{
		Type[] Hubs { get; }
	}
}