using System;
using System.Linq;
using Vertica.Integration.Infrastructure.Database;

namespace Vertica.Integration.Infrastructure.Threading
{
    /*
    internal class DbDistributedMutex : IDistributedMutex
    {
        private readonly IDbFactory _db;

        public DbDistributedMutex(IDbFactory db)
        {
            _db = db;
        }

        public IDisposable Enter(MutexContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return new LockTaker(_db, context);
        }

        private class LockTaker : IDisposable
        {
            private readonly DbLock _newLock;
            private readonly IDbSession _session;

            public LockTaker(IDbFactory db, MutexContext context)
            {
                _newLock = new DbLock
                {
                    Name = context.Name,
                    MachineName = Environment.MachineName
                };

                _session = db.OpenSession();

                while (retryCount-- > 0)
                {
                    DbLock currentLock;
                    if (TryTakeLock(out currentLock))
                        return;

                    //if (retryCount == 0)
                    //    throw new Exception($"Waited for {pauseBeteenRetries.Value} for another instance of the task {name} to complete. Giving up...");

                    //log.Message($"Another instance of the task is already running. Waiting for {pauseBeteenRetries.Value.ToString("h'h 'm'm 's's'")} and then try again");

                    //Thread.Sleep(pauseBeteenRetries.Value);
                }
            }
            
            private bool TryTakeLock(out DbLock currentLock)
            {
                currentLock = _session.Query<DbLock>(@"
BEGIN TRY
INSERT INTO Mutex (Name, LockedAt, MachineName, LockReason)
		VALUES (@Name, GETUTCDATE(), @MachineName, @LockReason)
END TRY

BEGIN CATCH
DECLARE @ErrorNumber AS INT
SELECT @ErrorNumber = ERROR_NUMBER();

IF @ErrorNumber = 2627 -- PRIMARY KEY VIOLATION
	SELECT * FROM Mutex WHERE (Name = @Name);
ELSE
	THROW
END CATCH
", _newLock).FirstOrDefault();

                return currentLock == null;
            }

            private void ReleaseLock()
            {
                _session.Execute(@"DELETE FROM Mutex WHERE Name = @Name", _newLock);
            }

            public void Dispose()
            {
                ReleaseLock();
                _session.Dispose();
            }

            public class DbLock
            {
                public string Name { get; set; }
                public string MachineName { get; set; }
                public DateTimeOffset LockedAt { get; set; }
            }
        }
    }
    */
}