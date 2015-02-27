using System;
using System.Data;
using System.Reflection;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;

namespace Vertica.Integration.Infrastructure.Database.Dapper.Castle.Windsor
{
	internal class DapperSelector<TConnection> : ITypedFactoryComponentSelector
		where TConnection : Connection
	{
		private readonly Connection _connection;

        public DapperSelector(Connection connection)
		{
			if (connection == null) throw new ArgumentNullException("connection");

			_connection = connection;
		}

		public Func<IKernelInternal, IReleasePolicy, object> SelectComponent(MethodInfo method, Type type, object[] arguments)
		{
			return (kernel, policy) =>
			{
                if (method.ReturnType == typeof(IDapperSession))
                    return kernel.Resolve(_connection.SessionName, method.ReturnType);

                if (method.ReturnType == typeof(IDbConnection))
                    return kernel.Resolve(_connection.DbConnectionName, method.ReturnType);

				throw new NotSupportedException(
					String.Format("Method '{0}' in type '{1}' not supported.", method.Name, type.FullName));
			};
		}
	}
}