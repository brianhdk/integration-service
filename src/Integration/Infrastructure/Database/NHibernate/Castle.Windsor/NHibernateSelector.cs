using System;
using System.Reflection;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;
using NHibernate;

namespace Vertica.Integration.Infrastructure.Database.NHibernate.Castle.Windsor
{
	internal class NHibernateSelector<TConnection> : ITypedFactoryComponentSelector
		where TConnection : Connection
	{
		private readonly Connection _connection;

		public NHibernateSelector(Connection connection)
		{
			if (connection == null) throw new ArgumentNullException("connection");

			_connection = connection;
		}

		public Func<IKernelInternal, IReleasePolicy, object> SelectComponent(MethodInfo method, Type type, object[] arguments)
		{
			return (kernel, policy) =>
			{
				if (method.ReturnType == typeof(ISessionFactory))
					return kernel.Resolve(_connection.SessionFactoryName, method.ReturnType);

				if (method.ReturnType == typeof(ICurrentSession))
					return kernel.Resolve(_connection.CurrentSessionName, method.ReturnType);

				throw new NotSupportedException(
					String.Format("Method '{0}' in type '{1}' not supported.", method.Name, type.FullName));
			};
		}
	}
}