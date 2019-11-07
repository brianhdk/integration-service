using System;
using Castle.MicroKernel;
using Vertica.Integration.Infrastructure.IO;

namespace Vertica.Integration.Domain.LiteServer
{
    public class BackgroundServerContext
	{
        internal BackgroundServerContext(IKernel kernel)
        {
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));

            Console = kernel.Resolve<IConsoleWriter>();
        }

        /// <summary>
        /// Gives access to the <see cref="IConsoleWriter"/>.
        /// </summary>
	    public IConsoleWriter Console { get; }
    }
}