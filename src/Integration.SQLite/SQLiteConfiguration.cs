using System;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Database;

namespace Vertica.Integration.SQLite
{
    public class SQLiteConfiguration : IInitializable<IWindsorContainer>
    {
        internal SQLiteConfiguration(DatabaseConfiguration database)
        {
            if (database == null) throw new ArgumentNullException("database");

			Application = database.Application;

			// skift databasen ud.
        }

        public ApplicationConfiguration Application { get; private set; }

	    void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
	    {
	    }
    }
}