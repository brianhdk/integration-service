using System;

namespace Vertica.Integration.IIS
{
    public static class IISExtensions
    {
        public static ApplicationConfiguration UseIIS(this ApplicationConfiguration application, Action<IISConfiguration> iis = null)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

            return application.Extensibility(extensibility =>
            {
                IISConfiguration configuration = extensibility.Register(() => new IISConfiguration(application));

                iis?.Invoke(configuration);
            });
        }
    }
}