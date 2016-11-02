using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Features;
using Vertica.Integration.Infrastructure.Threading;
using Vertica.Integration.Infrastructure.Threading.DistributedMutex;
using Vertica.Integration.Infrastructure.Threading.DistributedMutex.Db;

namespace Vertica.Integration.Tests.Infrastructure.Threading
{
    [TestFixture(Category = "Slow,Integration")]
    public class DbDistributedMutexIntegrationTester
    {
        [Test]
        public void Complex_DifferentLocks_MultiThreaded()
        {
            var db = Substitute.For<IDbFactory>();
            var dbConfiguration = Substitute.For<IIntegrationDatabaseConfiguration>();
            var session = Substitute.For<IDbSession>();
            var settings = Substitute.For<IRuntimeSettings>();
            var featureToggler = Substitute.For<IFeatureToggler>();
            var shutdown = Substitute.For<IShutdown>();

            settings[DbDistributedMutex.QueryLockIntervalKey].Returns(TimeSpan.FromSeconds(1).ToString());

            db.OpenSession().Returns(session);

            var subject = new DbDistributedMutex(db, dbConfiguration, settings, featureToggler, shutdown);

            var locks = new ConcurrentDictionary<string, DbDistributedMutexLock>();

            session
                .Query<DbDistributedMutexLock>(Arg.Any<string>(), Arg.Any<DbDistributedMutexLock>())
                .Returns(x =>
                {
                    var newLock = x.Arg<DbDistributedMutexLock>();

                    DbDistributedMutexLock existingLock = locks.GetOrAdd(newLock.Name, key => newLock);

                    if (existingLock.Equals(newLock))
                        return new DbDistributedMutexLock[0];

                    return new[] { existingLock };
                });

            session.Execute(Arg.Any<string>(), Arg.Any<DbDistributedMutexLock>()).Returns(x =>
            {
                var removeLock = x.Arg<DbDistributedMutexLock>();

                DbDistributedMutexLock removedLock;
                bool removed = locks.TryRemove(removeLock.Name, out removedLock);

                Assert.IsTrue(removed);
                Assert.That(removeLock, Is.EqualTo(removedLock));

                return 1;
            });

            var configuration = new DistributedMutexConfiguration(TimeSpan.FromSeconds(3));

            Func<string, TimeSpan, Task> startTask = (name, timeToExecute) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    int id = Thread.CurrentThread.ManagedThreadId;

                    using (subject.Enter(new DistributedMutexContext(name, configuration, x => Console.WriteLine($"Thread {id} waiting: {x}"))))
                    {
                        Console.WriteLine($"Thread {id} entered {name}");

                        Thread.Sleep(timeToExecute);

                        Console.WriteLine($"Thread {id} exitting {name}");
                    }
                });
            };

            // Start A - and allow for it to begin its work
            Task taskA = startTask("Lock1", TimeSpan.FromSeconds(1));
            Thread.Sleep(100);

            // Start B and C at the same time - they'll each wait on A to finish (1 second) 
            //  - and then wait for the one of them that got started first
            Task taskB = startTask("Lock1", TimeSpan.FromSeconds(1));
            Task taskC = startTask("Lock1", TimeSpan.FromSeconds(1));

            // Start a task using a completely different lock
            Task taskD = startTask("Lock2", TimeSpan.FromSeconds(1));

            // Start a task that takes longer than the wait-time
            Task taskE = startTask("Lock3", configuration.WaitTime + TimeSpan.FromSeconds(1));
            Thread.Sleep(100);
            Task taskF = startTask("Lock3", TimeSpan.FromSeconds(1));

            var allTasks = new[] { taskA, taskB, taskC, taskD, taskE, taskF };

            try
            {
                Task.WaitAll(allTasks);
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> inner = ex.Flatten().InnerExceptions;

                if (inner.Count > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine(@"--- Exceptions ---");
                    Console.WriteLine();

                    foreach (var innerException in inner)
                        Console.WriteLine(innerException.Message);
                }
            }

            // we expected this to timeout
            Assert.IsNotNull(taskF.Exception);
            Assert.IsInstanceOf<DistributedMutexTimeoutException>(taskF.Exception.InnerExceptions[0]);
        }
    }
}