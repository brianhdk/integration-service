namespace Vertica.Integration.Properties
{
	public interface ISettings
	{
		string WindowsServiceUsername { get; }
		string WindowsServicePassword { get; }
		string WindowsServiceNamePrefix { get; }

		string EmailSubjectFormat { get; }
	}

	public partial class Settings : ISettings
	{
	}
}