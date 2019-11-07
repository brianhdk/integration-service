using System;

namespace Vertica.Integration.Domain.LiteServer
{
	public interface ILiteServerFactory
	{
		IDisposable Create();
	}
}