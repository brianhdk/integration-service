using NHibernate;

namespace Vertica.Integration.Infrastructure.Database
{
    internal sealed class Db : PetaPoco.Database, IDb
    {
        private readonly IStatelessSession _session;

        public Db(IStatelessSession session)
            : base(session.Connection)
        {
            _session = session;
        }

        public override void Dispose()
        {
            base.Dispose();
            _session.Dispose();
        }
    }
}