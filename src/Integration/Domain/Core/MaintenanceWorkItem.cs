﻿using System;

namespace Vertica.Integration.Domain.Core
{
	public class MaintenanceWorkItem
	{
	    public MaintenanceWorkItem(MaintenanceConfiguration configuration)
	    {
	        if (configuration == null) throw new ArgumentNullException("configuration");

	        Configuration = configuration;
	    }

	    public MaintenanceConfiguration Configuration { get; private set; }
	}
}