using System;
using Castle.MicroKernel.Registration;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor
{
	public static class CastleWindsorExtensions
    {
        public static BasedOnDescriptor Expose(this BasedOnDescriptor descriptor, Action<Type> expose)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            if (expose == null) throw new ArgumentNullException(nameof(expose));

            return descriptor.If(x =>
            {
                expose(x);
                return true;
            });
        }
    }
}