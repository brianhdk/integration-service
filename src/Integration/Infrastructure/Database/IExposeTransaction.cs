using System.Data;

namespace Vertica.Integration.Infrastructure.Database
{
    public interface IExposeTransaction
    {
        IDbTransaction Transaction { get; }
    }
}