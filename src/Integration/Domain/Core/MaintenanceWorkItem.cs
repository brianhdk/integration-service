using System;
using Vertica.Integration.Model;

namespace Vertica.Integration.Domain.Core
{
    public class MaintenanceWorkItem : ContextWorkItem
	{
	    public MaintenanceWorkItem(MaintenanceConfiguration configuration)
	    {
	        if (configuration == null) throw new ArgumentNullException("configuration");

	        Configuration = configuration;
	    }

	    public MaintenanceConfiguration Configuration { get; private set; }
	}
}