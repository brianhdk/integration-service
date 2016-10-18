using System;
using Castle.Windsor;
using FluentMigrator;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.UCommerce.Database;

namespace Vertica.Integration.UCommerce
{
	public class UCommerceConfiguration :
        IInitializable<ApplicationConfiguration>,
        IInitializable<IWindsorContainer>
	{
	    private UCommerceDb _connection;
	    private Action<ConnectionString> _migration;

	    internal UCommerceConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

			Application = application
				.AddCustomInstaller(Install.ByConvention
					.AddFromAssemblyOfThis<UCommerceConfiguration>()
					.Ignore<UCommerceConfiguration>());
		}

		public UCommerceConfiguration Change(Action<UCommerceConfiguration> change)
		{
			change?.Invoke(this);

			return this;
		}

		public ApplicationConfiguration Application { get; }

        public UCommerceConfiguration Database(ConnectionString connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            return Database(new UCommerceDb(connectionString));
        }

        public UCommerceConfiguration Database<TConnection>(TConnection connection)
            where TConnection : UCommerceDb
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            _connection = connection;

            return this;
        }

        internal void AddMigrationFromNamespaceOfThis<T>(DatabaseServer db, string identifyingName)
            where T : Migration
        {
            _migration = connectionString =>
            {
                Application.Migration(migration => migration
                    .AddFromNamespaceOfThis<T>(db, connectionString, identifyingName));
            };
        }

        void IInitializable<ApplicationConfiguration>.Initialize(ApplicationConfiguration application)
        {
            if (_connection == null)
                Database(ConnectionString.FromName("uCommerceDb"));

            if (_connection != null)
            {
                Application.Database(database => database.AddConnection(_connection));

                _migration?.Invoke(_connection.ConnectionString);
            }
        }

	    void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
		}
	}
}