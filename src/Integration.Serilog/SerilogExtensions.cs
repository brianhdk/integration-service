using System;

namespace Vertica.Integration.Serilog
{
    public static class SerilogExtensions
    {
        public static ApplicationConfiguration UseSerilog(this ApplicationConfiguration application, Action<SerilogConfiguration> serilog)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));
	        if (serilog == null) throw new ArgumentNullException(nameof(serilog));

	        return application.Extensibility(extensibility =>
	        {
				SerilogConfiguration configuration = 
                    extensibility.Register(() => new SerilogConfiguration(application));

				serilog(configuration);
	        });
        }
    }
}