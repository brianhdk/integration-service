using System;

namespace Vertica.Integration.Hangfire
{
	public interface IHangfireServerFactory
	{
		IDisposable Create();
	}
}