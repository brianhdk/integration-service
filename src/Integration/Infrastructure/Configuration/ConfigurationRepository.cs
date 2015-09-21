using System;
using System.Data;
using System.Linq;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Infrastructure.Configuration
{
	public class ConfigurationRepository : IConfigurationRepository
	{
		private readonly Func<IDbFactory> _db;

		public ConfigurationRepository(Func<IDbFactory> db)
		{
			_db = db;
		}

		public Configuration[] GetAll()
		{
			using (IDbSession session = OpenSession())
			{
				return
					session
						.Query<Configuration>("SELECT Id, Name, Created, Updated, UpdatedBy FROM Configuration")
						.ToArray();
			}
		}

		public Configuration Get(string id)
		{
			if (String.IsNullOrWhiteSpace(id)) throw new ArgumentException(@"Value cannot be null or empty.", "id");

			using (IDbSession session = OpenSession())
			{
				return
					session
						.Query<Configuration>(
							"SELECT Id, Name, Description, JsonData, Created, Updated, UpdatedBy FROM Configuration WHERE (Id = @id)",
							new { id })
						.SingleOrDefault();
			}
		}

		public Configuration Save(Configuration configuration)
		{
			if (configuration == null) throw new ArgumentNullException("configuration");

			configuration.Name = configuration.Name.MaxLength(50);
			configuration.Description = configuration.Description.MaxLength(255);
			configuration.Updated = Time.UtcNow;
			configuration.UpdatedBy = configuration.UpdatedBy.MaxLength(50);

			using (IDbSession session = OpenSession())
			using (IDbTransaction transaction = session.BeginTransaction())
			{
				session.Execute(@"
IF NOT EXISTS (SELECT Id FROM Configuration WHERE (Id = @Id))
	BEGIN
		INSERT INTO Configuration (Id, Name, Description, JsonData, Created, Updated, UpdatedBy)
			VALUES (@Id, @Name, @Description, @JsonData, @Updated, @Updated, @UpdatedBy);
	END
ELSE
	BEGIN
		UPDATE Configuration SET
			JsonData = @JsonData,
			Updated = @Updated,
            UpdatedBy = @UpdatedBy,
            Name = @Name,
			Description = @Description
		WHERE (Id = @Id)
	END
", configuration);

				transaction.Commit();
			}

			return configuration;
		}

		public void Delete(string id)
		{
			if (String.IsNullOrWhiteSpace(id)) throw new ArgumentException(@"Value cannot be null or empty.");

			using (IDbSession session = OpenSession())
			using (IDbTransaction transaction = session.BeginTransaction())
			{
				session.Execute("DELETE FROM Configuration WHERE (Id = @id)", new { id });
				transaction.Commit();
			}
		}

		private IDbSession OpenSession()
		{
			return _db().OpenSession();
		}
	}
}