using System;

namespace Vertica.Integration.WebApi.Infrastructure
{
	public interface IHttpServerFactory
	{
		IDisposable Create(string url);
	}
}