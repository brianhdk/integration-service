using System.Collections.Specialized;

namespace Vertica.Integration.Properties
{
	public interface ISettings
	{
        bool DisableDatabaseLog { get; }

		string WindowsServiceUsername { get; }
		string WindowsServicePassword { get; }
		string WindowsServiceNamePrefix { get; }

		string EmailSubjectFormat { get; }

		StringCollection MonitorEmailRecipientsForService { get; }
		StringCollection MonitorEmailRecipientsForBusiness { get; }
        StringCollection MonitorEmailRecipientsForBusinessFreightRelated { get; }

	    StringCollection IgnoreErrorsWithMessagesContaining { get; }
	}

	public partial class Settings : ISettings
	{
	}
}