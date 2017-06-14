using System;
using System.Data;
using System.Linq;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Database.Extensions;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Utilities;

namespace Vertica.Integration.Infrastructure.Archiving
{
	internal class DbArchiveService : IArchiveService
    {
        private readonly IDbFactory _db;
	    private readonly IIntegrationDatabaseConfiguration _configuration;
	    private readonly int _deleteBatchSize;

        public DbArchiveService(IDbFactory db, IIntegrationDatabaseConfiguration configuration, IRuntimeSettings settings)
        {
            _db = db;
            _configuration = configuration;

            if (!int.TryParse(settings[$"{nameof(DbArchiveService)}.DeleteBatchSize"], out _deleteBatchSize))
                _deleteBatchSize = 20;
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

                    archiveId = session.Wrap(s => s.ExecuteScalar<int>($@"
INSERT INTO [{_configuration.TableName(IntegrationDbTable.Archive)}] (Name, BinaryData, ByteSize, Created, Expires, GroupName) VALUES (@name, @binaryData, @byteSize, @created, @expires, @groupName);
SELECT CAST(SCOPE_IDENTITY() AS INT);",
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

				onCreated?.Invoke(new ArchiveCreated(archiveId.ToString()));
			});
        }

        public Archive[] GetAll()
        {
			using (IDbSession session = OpenSession())
            {
                return session.Wrap(s => s.Query<Archive>($@"
SELECT 
    Id, 
    Name, 
    ByteSize, 
    Created, 
    GroupName, 
    Expires 
FROM [{_configuration.TableName(IntegrationDbTable.Archive)}]")).ToArray();
            }
        }

        public byte[] Get(string id)
        {
            int value;
            if (!int.TryParse(id, out value))
                return null;

			using (IDbSession session = OpenSession())
            {
                return
                    session.Wrap(s => s.Query<byte[]>($@"
SELECT BinaryData FROM [{_configuration.TableName(IntegrationDbTable.Archive)}] WHERE Id = @Id", new { Id = value })).SingleOrDefault();
            }
        }

        public int Delete(DateTimeOffset olderThan)
        {
			using (IDbSession session = OpenSession())
            {
                int count = session.Wrap(s => s.Execute(DeleteSql("Created <= @olderThan"),
                    new { batchSize = _deleteBatchSize, olderThan }, commandTimeout: 0));

                return count;
            }
        }

        public int DeleteExpired()
        {
			using (IDbSession session = OpenSession())
            {
                int count = session.Wrap(s => s.Execute(DeleteSql("Expires <= @now"), 
                    new { batchSize = _deleteBatchSize, now = Time.UtcNow }, commandTimeout: 0));

                return count;
            }
        }

	    private string DeleteSql(string where)
	    {
	        return $@"
DECLARE @DELETEDCOUNT INT
SET @DELETEDCOUNT = 0

DECLARE @BATCHCOUNT INT
SET @BATCHCOUNT = @batchSize

WHILE @BATCHCOUNT > 0
BEGIN

    DELETE TOP(@BATCHCOUNT) FROM [{_configuration.TableName(IntegrationDbTable.Archive)}] WHERE ({where})
    SET @BATCHCOUNT = @@ROWCOUNT
    SET @DELETEDCOUNT = @DELETEDCOUNT + @BATCHCOUNT

	IF @BATCHCOUNT > 0
	BEGIN
		WAITFOR DELAY '00:00:02' -- wait 2 secs to allow other processes access to the table
	END
END

SELECT @DELETEDCOUNT";
	    }

		private IDbSession OpenSession()
		{
			return _db.OpenSession();
		}
    }
}