using System.Data;

namespace Vertica.Integration.Infrastructure.Database.Dapper
{
    public interface IExposeTransaction
    {
        IDbTransaction Transaction { get; }
    }
}