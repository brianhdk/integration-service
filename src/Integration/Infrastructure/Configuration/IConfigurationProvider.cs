namespace Vertica.Integration.Infrastructure.Configuration
{
    public interface IConfigurationProvider
    {
        TConfiguration Get<TConfiguration>()
            where TConfiguration : class, new();

        void Save<TConfiguration>(TConfiguration configuration, string updatedBy, bool createArchiveBackup = false);

        Configuration[] GetAll();
        Configuration Get(string clrType);
        Configuration Save(Configuration configuration);
    }
}