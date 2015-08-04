using System;
using System.Text;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Database.Databases;

namespace Vertica.Integration.Infrastructure.Database.Castle.Windsor
{
    internal class DbInstaller : DbInstaller<DefaultConnection>
	{
		public DbInstaller(DefaultConnection connection)
			: base(connection)
		{
		}

		public override void Install(IWindsorContainer container, IConfigurationStore store)
		{
            if (container.Kernel.HasComponent(typeof(IDbFactory)))
                throw new InvalidOperationException("Only one DefaultConnection can be installed. Use the generic installer for additional instances.");

			base.Install(container, store);

		    container.Register(
		        Component.For<IDbFactory>()
		            .UsingFactoryMethod(kernel => new DbFactory(kernel.Resolve<IDbFactory<DefaultConnection>>())));
		}
	}

	public class DbInstaller<TConnection> : IWindsorInstaller
		where TConnection : Connection
	{
		private readonly TConnection _connection;

	    public DbInstaller(TConnection connection)
		{
			if (connection == null) throw new ArgumentNullException("connection");

			_connection = connection;
		}

		public virtual void Install(IWindsorContainer container, IConfigurationStore store)
		{
            var disabled = _connection as IDisabledConnection;

		    if (disabled != null)
		    {
		        container.Register(
		            Component.For<IDbFactory<TConnection>>()
		                .UsingFactoryMethod<IDbFactory<TConnection>>((kernel, model, context) =>
		                {
		                    var sb = new StringBuilder();
		                    sb.AppendLine(disabled.ExceptionMessage);
		                    sb.AppendLine();

		                    sb.AppendLine("Examine the DependencyChain below to see which component has a dependency of this:");
		                    sb.AppendLine();

		                    context.BuildCycleMessageFor(context.Handler, sb);

		                    throw new DatabaseDisabledException(sb.ToString());
		                }));
		    }
		    else
		    {
                container.Register(
                    Component.For<IDbFactory<TConnection>>()
                        .UsingFactoryMethod(() => new DbFactory<TConnection>(_connection)));
		    }
		}
	}
}