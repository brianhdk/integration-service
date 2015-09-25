using System;
using System.Data;
using Castle.MicroKernel;

namespace Vertica.Integration.Infrastructure.Database
{
	public sealed class DefaultConnection : Connection
	{
		private readonly bool _isDisabled;
		private readonly Connection _connection;

		private DefaultConnection()
			: base(ConnectionString.FromText(String.Empty))
		{
			_isDisabled = true;
		}

		internal DefaultConnection(ConnectionString connectionString)
			: base(connectionString)
		{
		}

		internal DefaultConnection(Connection connection)
			: base(connection.ConnectionString)
		{
			if (connection == null) throw new ArgumentNullException("connection");

			_connection = connection;
		}

		protected internal override IDbConnection GetConnection(IKernel kernel)
		{
			if (_connection != null)
				return _connection.GetConnection(kernel);
					
			return base.GetConnection(kernel);
		}

		internal bool IsDisabled
		{
			get { return _isDisabled; }
		}

		public static DefaultConnection Disabled
		{
			get { return new DefaultConnection(); }
		}
	}
}