using System;
using System.Text;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Vertica.Integration.Infrastructure.Database.Castle.Windsor
{
    internal class DbInstaller : DbInstaller<DefaultConnection>
	{
		public DbInstaller(DefaultConnection connection)
			: base(connection, connection.IsDisabled)
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

	internal class DbInstaller<TConnection> : IWindsorInstaller
		where TConnection : Connection
	{
		private readonly TConnection _connection;
		private readonly bool _isDisabled;

		public DbInstaller(TConnection connection, bool isDisabled = false)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			_connection = connection;
			_isDisabled = isDisabled;
		}

		public virtual void Install(IWindsorContainer container, IConfigurationStore store)
		{
			if (container.Kernel.HasComponent(typeof(IDbFactory<TConnection>)))
				throw new InvalidOperationException($"Only one {typeof (TConnection).FullName} can be installed.");

		    if (_isDisabled)
		    {
		        container.Register(
		            Component.For<IDbFactory<TConnection>>()
		                .UsingFactoryMethod<IDbFactory<TConnection>>((kernel, model, context) =>
		                {
		                    var sb = new StringBuilder();
							sb.AppendLine("DbFactory has been disabled.");
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
                        .UsingFactoryMethod(kernel => new DbFactory<TConnection>(_connection, kernel)));
		    }
		}
	}
}