using System;
using System.Data;
using System.Linq;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Database.Extensions;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Infrastructure.Archiving
{
	internal class DbArchiveService : IArchiveService
    {
        private readonly Func<IDbFactory> _db;

        public DbArchiveService(Func<IDbFactory> db)
        {
            _db = db;
        }

        public BeginArchive Create(string name, Action<ArchiveCreated> onCreated = null)
        {
            return new BeginArchive(name, (stream, options) =>
            {
                int archiveId;

				using (IDbSession session = OpenSession())
                using (IDbTransaction transaction = session.BeginTransaction())
                {
                    byte[] binaryData = stream.ToArray();

                    archiveId = session.Wrap(s => s.ExecuteScalar<int>(
                        "INSERT INTO Archive (Name, BinaryData, ByteSize, Created, Expires, GroupName) VALUES (@name, @binaryData, @byteSize, @created, @expires, @groupName);" +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);",
                        new 
                        {
                            name = options.Name.MaxLength(255),
                            binaryData,
                            byteSize = binaryData.Length,
                            created = Time.UtcNow,
                            expires = options.Expires,
                            groupName = options.GroupName
                        }));

                    transaction.Commit();
                }

                if (onCreated != null)
                    onCreated(new ArchiveCreated(archiveId.ToString()));
            });
        }

        public Archive[] GetAll()
        {
			using (IDbSession session = OpenSession())
            {
                return session.Wrap(s => s.Query<Archive>("SELECT Id, Name, ByteSize, Created, GroupName, Expires FROM Archive"))
                    .ToArray();
            }
        }

        public byte[] Get(string id)
        {
            int value;
            if (!Int32.TryParse(id, out value))
                return null;

			using (IDbSession session = OpenSession())
            {
                return
                    session.Wrap(s => s.Query<byte[]>("SELECT BinaryData FROM Archive WHERE Id = @Id", new { Id = value }))
                        .SingleOrDefault();
            }
        }

        public int Delete(DateTimeOffset olderThan)
        {
			using (IDbSession session = OpenSession())
            using (IDbTransaction transaction = session.BeginTransaction())
            {
                int count = session.Wrap(s => s.Execute("DELETE FROM Archive WHERE Created <= @olderThan", new { olderThan }));

                transaction.Commit();

                return count;
            }
        }

        public int DeleteExpired()
        {
			using (IDbSession session = OpenSession())
            using (IDbTransaction transaction = session.BeginTransaction())
            {
                int count = session.Wrap(s => s.Execute("DELETE FROM Archive WHERE Expires <= @now", new { now = Time.UtcNow }));

                transaction.Commit();

                return count;
            }
        }

		private IDbSession OpenSession()
		{
			return _db().OpenSession();
		}
    }
}