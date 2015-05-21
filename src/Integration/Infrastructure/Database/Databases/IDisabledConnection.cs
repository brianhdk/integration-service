namespace Vertica.Integration.Infrastructure.Database.Databases
{
    internal interface IDisabledConnection
    {
        string ExceptionMessage { get; }
    }
}