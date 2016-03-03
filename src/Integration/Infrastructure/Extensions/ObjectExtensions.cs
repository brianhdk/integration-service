using System;

namespace Vertica.Integration.Infrastructure.Extensions
{
	internal static class ObjectExtensions
	{
		public static void DisposeIfDisposable(this object instance)
		{
			var disposable = instance as IDisposable;
			disposable?.Dispose();
		}
	}
}