namespace Vertica.Integration.Infrastructure.Windows
{
	public class WindowsFactory : IWindowsFactory
	{
		public IWindowsServices WindowsServices(string machineName = null)
		{
			return new WindowsServices(machineName);
		}
	}
}