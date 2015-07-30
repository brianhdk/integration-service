using System;

namespace Vertica.Integration.Model.Web
{
	public interface IHttpServerFactory
	{
		IDisposable Create(string url);
	}
}