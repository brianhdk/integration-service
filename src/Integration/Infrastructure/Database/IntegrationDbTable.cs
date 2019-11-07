namespace Vertica.Integration.Infrastructure.Database
{
    public enum IntegrationDbTable
    {
        TaskLog,
        ErrorLog,
        Archive,
        Configuration,
        DistributedMutex,

        // ReSharper disable once InconsistentNaming
        BuiltIn_VersionInfo
    }
}