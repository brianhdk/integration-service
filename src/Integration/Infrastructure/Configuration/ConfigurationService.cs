using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Utilities_v4;
using Vertica.Utilities_v4.Extensions.AttributeExt;
using Vertica.Utilities_v4.Extensions.StringExt;

namespace Vertica.Integration.Infrastructure.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IDbFactory _db;
        private readonly IArchiveService _archive;
        private readonly ILogger _logger;

        public ConfigurationService(IDbFactory db, IArchiveService archive, ILogger logger)
        {
            _archive = archive;
            _logger = logger;
            _db = db;
        }

        public TConfiguration Get<TConfiguration>() where TConfiguration : class, new()
        {
            if (typeof (TConfiguration) == typeof(Configuration)) throw new ArgumentException("Getting a Configuration of type Configuration is not allowed.");

            Configuration existing = Get(GetId<TConfiguration>());

            if (existing != null)
            {
                return JsonConvert.DeserializeObject<TConfiguration>(existing.JsonData);
            }

            var configuration = new TConfiguration();
            Save(configuration, "IntegrationService");

            return configuration;
        }

        public TConfiguration Save<TConfiguration>(TConfiguration configuration, string updatedBy, bool createArchiveBackup = false) 
            where TConfiguration : class, new()
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (String.IsNullOrWhiteSpace(updatedBy)) throw new ArgumentException(@"Value cannot be null or empty.", "updatedBy");
            if (configuration is Configuration) throw new ArgumentException(@"Use the specific Save method when saving this Configuration instance.", "configuration");

            SaveInternal(
                GetId<TConfiguration>(warnIfMissingGuid: true),
                JsonConvert.SerializeObject(configuration, Formatting.Indented),
                updatedBy,
                createArchiveBackup,
                typeof(TConfiguration));

            return Get<TConfiguration>();
        }

        public Configuration[] GetAll()
        {
            using (IDbSession session = _db.OpenSession())
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

            using (IDbSession session = _db.OpenSession())
            {
                return
                    session
                        .Query<Configuration>(
                            "SELECT Id, Name, Description, JsonData, Created, Updated, UpdatedBy FROM Configuration WHERE (Id = @id)",
                            new { id })
                        .SingleOrDefault();
            }
        }

        public Configuration Save(Configuration configuration, string updatedBy, bool createArchiveBackup = false)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            SaveInternal(configuration.Id, configuration.JsonData, updatedBy, createArchiveBackup);

            return Get(configuration.Id);
        }

        public void Delete(string id)
        {
            if (String.IsNullOrWhiteSpace(id)) throw new ArgumentException(@"Value cannot be null or empty.");

            using (IDbSession session = _db.OpenSession())
            using (IDbTransaction transaction = session.BeginTransaction())
            {
                session.Execute("DELETE FROM Configuration WHERE (Id = @id)", new {id});
                transaction.Commit();
            }
        }

        private void SaveInternal(string id, string jsonData, string updatedBy, bool createArchiveBackup, Type configurationType = null)
        {
            string name = (configurationType != null ? configurationType.Name : id).MaxLength(100);

            if (createArchiveBackup)
            {
                Configuration configuration = Get(id);

                if (configuration != null)
                {
                    _archive.Archive(String.Format("[Backup] {0}", configuration.Name), archive =>
                    {
                        archive.IncludeContent("data", configuration.JsonData, ".json");
                        archive.IncludeContent("meta", String.Join(Environment.NewLine,
                            configuration.Id,
                            configuration.Name,
                            configuration.Description,
                            configuration.Updated.ToString(),
                            configuration.UpdatedBy));
                    });
                }
            }

            using (IDbSession session = _db.OpenSession())
            using (IDbTransaction transaction = session.BeginTransaction())
            {
                string description = GetDescription(configurationType).MaxLength(255);

                string updateNameDescriptionSql = configurationType != null
                    ? ", Name = @name, Description = @description"
                    : String.Empty;

                session.Execute(String.Format(@"
IF NOT EXISTS (SELECT Id FROM Configuration WHERE (Id = @id))
	BEGIN
		INSERT INTO Configuration (Id, Name, Description, JsonData, Created, Updated, UpdatedBy)
			VALUES (@id, @name, @description, @jsonData, @updated, @updated, @updatedBy);
	END
ELSE
	BEGIN
		UPDATE Configuration SET
			JsonData = @jsonData,
			Updated = @updated,
            UpdatedBy = @updatedBy
            {0}
		WHERE (Id = @id)
	END
", updateNameDescriptionSql), new { id, name, description, jsonData, updated = Time.UtcNow, updatedBy });

                transaction.Commit();
            }
        }

        private string GetDescription(Type configurationType)
        {
            if (configurationType == null)
                return null;

            DescriptionAttribute attribute = 
                configurationType.GetAttribute<DescriptionAttribute>();

            return attribute != null ? attribute.Description.NullIfEmpty() : null;
        }

        private string GetId<TConfiguration>(bool warnIfMissingGuid = false)
        {
            string id = GetGuidId<TConfiguration>();

            if (id != null)
                return id;

            Type type = typeof (TConfiguration);

            id = String.Join(", ", type.FullName, type.Assembly.GetName().Name);

            if (warnIfMissingGuid)
            {
                _logger.LogWarning(Target.Service,
@"Class '{0}' used for configuration should have been decorated with a [Guid(""[insert-new-Guid-here]"")]-attribute.
This is to ensure a unique and Refactor-safe Global ID.

Remember when (or if) you add this Guid-attribute, that you (manually) have to merge the data to the new instance.
If you don't like to do it manually, you can of course use a Migration.

IMPORTANT: Remember to use the ""D"" format for Guids, e.g. 1EB3F675-C634-412F-A76F-FC3F9A4A68D5", id);

                // TODO: Create an example for this on GitHub and link to that example.
            }

            return id;
        }

        internal static string GetGuidId<TConfiguration>()
        {
            Type type = typeof(TConfiguration);

            GuidAttribute attribute = type.GetAttribute<GuidAttribute>();

            Guid guid;
            if (attribute != null && Guid.TryParse(attribute.Value, out guid))
                return guid.ToString("D");

            return null;
        }
    }
}