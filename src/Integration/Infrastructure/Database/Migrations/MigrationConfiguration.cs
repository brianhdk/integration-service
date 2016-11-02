using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentMigrator;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    public class MigrationConfiguration
    {
	    private readonly MigrationDbs _dbs;

        internal MigrationConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

			Application = application
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Register<IMigrationDbs>(kernel => _dbs)));

            _dbs = new MigrationDbs();
        }

        public ApplicationConfiguration Application { get; }

        [Obsolete("Use .Database(database => database.IntegrationDb(integrationDb => integrationDb.ChangeDatabaseServer(...)))")]
        public MigrationConfiguration ChangeIntegrationDbDatabaseServer(DatabaseServer db)
        {
            Application.Database(database => database
                .IntegrationDb(integrationDb => integrationDb
                    .ChangeDatabaseServer(db)));

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
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

			return Add(new MigrationDb(
				db,
				connectionString,
				typeof(T).Assembly,
				typeof(T).Namespace,
				identifyingName));
        }

	    public MigrationConfiguration Add(MigrationDb migrationDb)
	    {
		    if (migrationDb == null) throw new ArgumentNullException(nameof(migrationDb));

		    _dbs.Add(migrationDb);

			return this;
	    }

        [Obsolete("Use .Database(database => database.IntegrationDb(integrationDb => integrationDb.DisableCheckExistsAndCreateDatabaseIfNotFound()))")]
        public MigrationConfiguration DisableCheckExistsAndCreateIntegrationDbIfNotFound()
        {
            Application.Database(database => database
                .IntegrationDb(integrationDb => integrationDb
                    .DisableCheckExistsAndCreateDatabaseIfNotFound()));

            return this;
        }
        
        private class MigrationDbs : IMigrationDbs
	    {
		    private readonly List<MigrationDb> _dbs;
		    private readonly List<Tuple<Type, string>> _types;

		    public MigrationDbs()
		    {
			    _dbs = new List<MigrationDb>();
			    _types = new List<Tuple<Type, string>>();
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
			    if (migrationDb == null) throw new ArgumentNullException(nameof(migrationDb));

			    _dbs.Add(migrationDb);
		    }

			public void Add(Type migration, string identifyingName)
			{
				if (migration == null) throw new ArgumentNullException(nameof(migration));

				_types.Add(Tuple.Create(migration, identifyingName));
			}

		    public IMigrationDbs WithIntegrationDb(IntegrationMigrationDb integrationDb)
		    {
			    if (integrationDb == null) throw new ArgumentNullException(nameof(integrationDb));

			    _dbs.Insert(0, integrationDb);

			    foreach (Tuple<Type, string> migration in _types)
				    _dbs.Insert(1, integrationDb.CopyTo(migration.Item1, migration.Item2));

			    return this;
		    }
	    }
    }
}