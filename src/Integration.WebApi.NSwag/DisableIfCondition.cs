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

        public IRuntimeSettings RuntimeSettings => Kernel.Resolve<IRuntimeSettings>();

        public bool IsProduction()
        {
            ApplicationEnvironment environment = RuntimeSettings.Environment;

            return environment.Equals(ApplicationEnvironment.Production);
        }

        /// <summary>
        /// Disable NSwag by specifying a value of 'True' for setting 'WebApi.NSwag.Disabled'.
        /// </summary>
        public bool IsDisabledByRuntimeSettings()
        {
            return string.Equals(
                RuntimeSettings["WebApi.NSwag.Disabled"], 
                bool.TrueString, 
                StringComparison.OrdinalIgnoreCase);
        }
    }
}