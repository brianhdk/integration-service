using System;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Database.Dapper;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Infrastructure.Configuration
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        private readonly IDapperProvider _dapper;
        private readonly IArchiver _archiver;

        public ConfigurationProvider(IDapperProvider dapper, IArchiver archiver)
        {
            _archiver = archiver;
            _dapper = dapper;
        }

        public TConfiguration Get<TConfiguration>() where TConfiguration : class, new()
        {
            if (typeof(TConfiguration) == typeof(Configuration)) throw new ArgumentException("Getting a Configuration of type Configuration is not allowed.");

            Configuration raw = Get(GetClrType(typeof (TConfiguration)));

            if (raw != null)
            {
                TConfiguration configuration = JsonConvert.DeserializeObject<TConfiguration>(raw.JsonData);

                return configuration;
            }

            return new TConfiguration();
        }

        public void Save<TConfiguration>(TConfiguration configuration, string updatedBy, bool createArchiveBackup = false) 
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (String.IsNullOrWhiteSpace(updatedBy)) throw new ArgumentException(@"Value cannot be null or empty.", "updatedBy");
            if (configuration is Configuration) throw new ArgumentException("Use the specific Save method when saving this Configuration instance.", "configuration");

            SaveInternal(
                GetClrType(typeof (TConfiguration)),
                JsonConvert.SerializeObject(configuration, Formatting.Indented),
                updatedBy,
                createArchiveBackup);
        }

        public Configuration[] GetAll()
        {
            using (IDapperSession session = _dapper.OpenSession())
            {
                return
                    session
                        .Query<Configuration>("SELECT ClrType, Created, Updated, UpdatedBy FROM Configuration")
                        .ToArray();
            }
        }

        public Configuration Get(string clrType)
        {
            if (String.IsNullOrWhiteSpace(clrType)) throw new ArgumentException(@"Value cannot be null or empty.", "clrType");

            using (IDapperSession session = _dapper.OpenSession())
            {
                return
                    session
                        .Query<Configuration>(
                            "SELECT ClrType, JsonData, Created, Updated, UpdatedBy FROM Configuration WHERE (ClrType = @ClrType)",
                            new { ClrType = clrType })
                        .SingleOrDefault();
            }
        }

        public Configuration Save(Configuration configuration, bool createArchiveBackup = false)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            SaveInternal(configuration.ClrType, configuration.JsonData, configuration.UpdatedBy, createArchiveBackup);

            return Get(configuration.ClrType);
        }

        private void SaveInternal(string clrType, string jsonData, string updatedBy, bool createArchiveBackup)
        {
            if (createArchiveBackup)
            {
                Configuration configuration = Get(clrType);

                if (configuration != null)
                {
                    _archiver.Archive(String.Format("Configuration.Backup-{0}", clrType), archive =>
                    {
                        archive.IncludeContent("data", configuration.JsonData, "json");
                        archive.IncludeContent("meta", String.Join(Environment.NewLine,
                            configuration.ClrType,
                            configuration.Updated.ToString(),
                            configuration.UpdatedBy));
                    });
                }
            }

            using (IDapperSession session = _dapper.OpenSession())
            using (IDbTransaction transaction = session.BeginTransaction())
            {
                // merge statement
                session.Execute(@"
IF NOT EXISTS (SELECT ClrType FROM Configuration WHERE (ClrType = @clrType))
	BEGIN
		INSERT INTO Configuration (ClrType, JsonData, Created, Updated, UpdatedBy)
			VALUES (@clrType, @jsonData, @updated, @updated, @updatedBy);
	END
ELSE
	BEGIN
		UPDATE Configuration SET
			JsonData = @jsonData,
			Updated = @updated,
            UpdatedBy = @updatedBy
		WHERE (ClrType = @clrType)
	END
", new { clrType, jsonData, updated = Time.UtcNow, updatedBy });

                transaction.Commit();
            }            
        }

        private string GetClrType(Type type)
        {
            return String.Join(", ", type.FullName, type.Assembly.GetName().Name);
        }
    }
}