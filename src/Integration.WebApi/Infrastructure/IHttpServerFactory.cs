using System;
using System.Web.Http;
using Owin;

namespace Vertica.Integration.WebApi.Infrastructure
{
	public interface IHttpServerFactory
	{
		IDisposable Create(string url, Action<IAppBuilder, HttpConfiguration> configuration = null);
	}
}