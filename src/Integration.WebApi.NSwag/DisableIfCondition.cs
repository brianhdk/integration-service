using System;
using Castle.MicroKernel;

namespace Vertica.Integration.WebApi.NSwag
{
    public class DisableIfCondition
    {
        internal DisableIfCondition(IKernel kernel)
        {
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));

            Kernel = kernel;
        }

        public IKernel Kernel { get; }

        public bool IsProduction()
        {
            ApplicationEnvironment environment = Kernel.Resolve<IRuntimeSettings>().Environment;

            return environment.Equals(ApplicationEnvironment.Production);
        }
    }
}