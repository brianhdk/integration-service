namespace Vertica.Integration.Infrastructure.Database
{
    public interface IDbFactory
    {
        IDb OpenDatabase();
    }
}