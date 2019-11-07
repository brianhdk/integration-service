using System;

namespace Vertica.Integration.ConsoleHost
{
    public static class ConsoleHostExtensions
    {
        public static ApplicationConfiguration UseConsoleHost(this ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));
            
            return application.Extensibility(extensibility =>
            {
                extensibility.Register(() => new ConsoleHostConfiguration(application));
            });
        }
    }
}