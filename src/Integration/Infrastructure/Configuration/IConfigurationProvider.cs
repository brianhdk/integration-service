namespace Vertica.Integration.Infrastructure.Configuration
{
    public interface IConfigurationProvider
    {
        TConfiguration GetOrInstantiateNew<TConfiguration>()
            where TConfiguration : class, new();

        void Save<TConfiguration>(TConfiguration configuration, string updatedBy, bool createArchiveBackup = false)
            where TConfiguration : class, new();

        Configuration[] GetAll();
        Configuration Get(string clrType);
        void Save(Configuration configuration);
    }
}