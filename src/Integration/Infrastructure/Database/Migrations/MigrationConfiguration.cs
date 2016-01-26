using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Castle.Windsor;
using FluentMigrator;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    public class MigrationConfiguration : IInitializable<IWindsorContainer>
    {
	    private readonly MigrationDbs _dbs;

        internal MigrationConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException("application");

			Application = application;

            _dbs = new MigrationDbs();
        }

        public ApplicationConfiguration Application { get; private set; }

        public MigrationConfiguration ChangeIntegrationDbDatabaseServer(DatabaseServer db)
        {
	        _dbs.IntegrationDbDatabaseServer = db;
            return this;
        }

        /// <summary>
        /// Makes it possible to execute custom migrations where the VersionInfo table will be stored in the IntegrationDb.
        /// Tip: Inherit from <see cref="IntegrationMigration"/> to have access to all services from the Integration Service.
        /// </summary>
        public MigrationConfiguration AddFromNamespaceOfThis<T>(string identifyingName = null)
            where T : Migration
        {
	        _dbs.Add(typeof (T), identifyingName);
	        return this;
        }

	    /// <summary>
	    /// Makes it possible to execute custom migrations against any database.
	    /// </summary>
	    /// <param name="db">Specifies the version of the database where the migrations are executed against.</param>
	    /// <param name="connectionString">Specifies the ConnectionString to the database.</param>
	    /// <param name="identifyingName">Specifies a name you can use to identify this specific migration.</param>
	    public MigrationConfiguration AddFromNamespaceOfThis<T>(DatabaseServer db, ConnectionString connectionString, string identifyingName = null)
            where T : Migration
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");

			return Add(new MigrationDb(
				db,
				connectionString,
				typeof(T).Assembly,
				typeof(T).Namespace,
				identifyingName));
        }

	    public MigrationConfiguration Add(MigrationDb migrationDb)
	    {
		    if (migrationDb == null) throw new ArgumentNullException("migrationDb");

		    _dbs.Add(migrationDb);

			return this;		    
	    }

        public MigrationConfiguration DisableCheckExistsAndCreateIntegrationDbIfNotFound()
        {
	        _dbs.CheckExistsAndCreateIntegrationDbIfNotFound = false;

            return this;
        }

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
	        Application.Database(database => _dbs.IntegrationDbDisabled = database.IntegrationDbDisabled);
            container.RegisterInstance<IMigrationDbs>(_dbs);
        }

	    private class MigrationDbs : IMigrationDbs
	    {
		    private readonly List<MigrationDb> _dbs;
		    private readonly List<Tuple<Type, string>> _types;

		    public MigrationDbs()
		    {
			    _dbs = new List<MigrationDb>();
			    _types = new List<Tuple<Type, string>>();

			    IntegrationDbDatabaseServer = DatabaseServer.SqlServer2014;
			    CheckExistsAndCreateIntegrationDbIfNotFound = true;
		    }

		    IEnumerator IEnumerable.GetEnumerator()
		    {
			    return GetEnumerator();
		    }

		    public IEnumerator<MigrationDb> GetEnumerator()
		    {
			    return _dbs.Distinct().GetEnumerator();
		    }

		    public void Add(MigrationDb migrationDb)
		    {
			    if (migrationDb == null) throw new ArgumentNullException("migrationDb");

			    _dbs.Add(migrationDb);
		    }

			public void Add(Type migration, string identifyingName)
			{
				if (migration == null) throw new ArgumentNullException("migration");

				_types.Add(Tuple.Create(migration, identifyingName));
			}

			public bool IntegrationDbDisabled { get; set; }
		    public DatabaseServer IntegrationDbDatabaseServer { get; set; }
		    public bool CheckExistsAndCreateIntegrationDbIfNotFound { get; set; }

		    public IMigrationDbs WithIntegrationDb(IntegrationMigrationDb integrationDb)
		    {
			    if (integrationDb == null) throw new ArgumentNullException("integrationDb");

			    if (IntegrationDbDisabled)
				    throw new InvalidOperationException(@"IntegrationDb is disabled.");

			    _dbs.Insert(0, integrationDb);

			    foreach (Tuple<Type, string> migration in _types)
				    _dbs.Insert(1, integrationDb.CopyTo(migration.Item1, migration.Item2));

			    return this;
		    }
	    }
    }
}