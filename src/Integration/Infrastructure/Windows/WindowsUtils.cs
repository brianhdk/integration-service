using System.Security.Principal;

namespace Vertica.Integration.Infrastructure.Windows
{
	public static class WindowsUtils
	{
		public static string GetIdentityName()
		{
			WindowsIdentity identity = WindowsIdentity.GetCurrent();

			if (identity != null)
				return identity.Name;

			return null;
		}		 
	}
}