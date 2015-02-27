using System;
using System.Data;
using System.Linq;
using Vertica.Integration.Infrastructure.Database.Dapper;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public class Archiver : IArchiver
    {
        private readonly IDapperProvider _dapper;

        public Archiver(IDapperProvider dapper)
        {
            _dapper = dapper;
        }

        public Archive Create(string name, Action<int> onCreated)
        {
            if (onCreated == null) throw new ArgumentNullException("onCreated");
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");

            return new Archive(stream =>
            {
                int archiveId;

                using (IDapperSession session = _dapper.OpenSession())
                using (IDbTransaction transaction = session.BeginTransaction())
                {
                    byte[] data = stream.ToArray();

                    archiveId = session.ExecuteScalar<int>(
                        "INSERT INTO Archive (Name, BinaryData, ByteSize, Created) VALUES (@Name, @BinaryData, @ByteSize, @Created);" +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);",
                        new
                        {
                            Name = name,
                            BinaryData = data,
                            ByteSize = data.Length,
                            Created = Time.UtcNow
                        });

                    transaction.Commit();
                }

                onCreated(archiveId);
            });
        }

        public SavedArchive[] GetAll()
        {
            using (IDapperSession session = _dapper.OpenSession())
            {
                return session.Query<SavedArchive>("SELECT Id, Name, ByteSize, Created FROM Archive")
                    .ToArray();
            }
        }

        public byte[] Get(int id)
        {
            using (IDapperSession session = _dapper.OpenSession())
            {
                return
                    session.Query<byte[]>("SELECT BinaryData FROM Archive WHERE Id = @Id", new { Id = id })
                        .SingleOrDefault();
            }
        }

        public int Delete(DateTime olderThan)
        {
            using (IDapperSession session = _dapper.OpenSession())
            using (IDbTransaction transaction = session.BeginTransaction())
            {
                int count = session.Execute("DELETE FROM Archive WHERE Created <= @olderThan", new { olderThan });

                transaction.Commit();

                return count;
            }
        }
    }
}