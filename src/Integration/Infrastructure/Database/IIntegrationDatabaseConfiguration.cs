namespace Vertica.Integration.Infrastructure.Database
{
    public interface IIntegrationDatabaseConfiguration
    {
        bool Disabled { get; }
        
        //string TablePrefix { get; }
    }
}