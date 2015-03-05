using System;
using Castle.MicroKernel.Registration;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor
{
    internal static class CastleWindsorExtensions
    {
        public static BasedOnDescriptor Expose(this BasedOnDescriptor descriptor, Action<Type> expose)
        {
            if (descriptor == null) throw new ArgumentNullException("descriptor");
            if (expose == null) throw new ArgumentNullException("expose");

            return descriptor.If(x =>
            {
                expose(x);
                return true;
            });
        }
    }
}