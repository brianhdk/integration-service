using System;

namespace Vertica.Integration.WebApi.Infrastructure
{
	public interface IHttpServerFactory
	{
		IDisposable Create(string url = null);

		string GetOrGenerateUrl(out bool basedOnSettings);
	}
}