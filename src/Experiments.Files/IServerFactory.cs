using System;

namespace Experiments.Files
{
	public interface IServerFactory
	{
		IDisposable Create();
	}
}