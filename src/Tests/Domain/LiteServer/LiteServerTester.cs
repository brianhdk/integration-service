using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Tests.Infrastructure.Testing;

namespace Vertica.Integration.Tests.Domain.LiteServer
{
    [TestFixture]
    public class LiteServerTester
    {
        [Test]
        public void Nothing_To_Do()
        {
            var logger = Substitute.For<ILogger>();

            using (var waitBlock = new WaitBlock())
            {
                var waitBlockLocal = new[] { waitBlock };

                using (var context = ApplicationContext.Create(application => application
                    .ConfigureForUnitTest()
                    .WithWaitBlock(waitBlockLocal[0])
                    .UseLiteServer(liteServer => liteServer.OnStartup(startup => { waitBlockLocal[0].Release(); }))
                    .Services(services => services
                        .Advanced(advanced => advanced
                            .Register(kernel => logger)))))
                {
                    context.Execute(nameof(LiteServerHost));
                }
            }
        }

        [Test]
        public void Worker_Failing_Ensure_Is_Logged()
        {
            var logger = Substitute.For<ILogger>();

            using (var waitBlock = new WaitBlock())
            {
                var waitBlockLocal = new[] { waitBlock };

                using (var context = ApplicationContext.Create(application => application
                    .ConfigureForUnitTest()
                    .WithWaitBlock(waitBlockLocal[0])
                    .UseLiteServer(liteServer => liteServer
                        .AddWorker<FailingWorker>())
                    .Services(services => services
                        .Advanced(advanced => advanced
                            .Register(kernel => logger)))))
                {
                    context.Execute(nameof(LiteServerHost));
                }
            }

            logger.Received(1).LogError(Arg.Is<FailingWorker.WorkException>(x => true));
        }

        [Test]
        public void RestartableWorker_Failing_EnsureIsRestarted()
        {
            var logger = Substitute.For<ILogger>();
            var state = new RestartableWorker.State(2);

            using (var waitBlock = new WaitBlock())
            {
                var waitBlockLocal = new[] { waitBlock };

                using (var context = ApplicationContext.Create(application => application
                    .ConfigureForUnitTest()
                    .WithWaitBlock(waitBlockLocal[0])
                    .UseLiteServer(liteServer => liteServer
                        .AddWorker<RestartableWorker>())
                    .Services(services => services
                        .Advanced(advanced => advanced
                            .Register(kernel => logger)
                            .Register(kernel => state)))))
                {
                    context.Execute(nameof(LiteServerHost));
                }
            }

            Assert.AreEqual(state.NumberOfRestarts, state.Context?.FailedCount);
            logger.Received((int) state.NumberOfRestarts).LogError(Arg.Is<RestartableWorker.WorkException>(x => true));
        }

        [Test]
        public void RestartableServer_Failing_EnsureIsRestarted()
        {
            var logger = Substitute.For<ILogger>();
            var state = new RestartableServer.State(2);

            using (var waitBlock = new WaitBlock())
            {
                var waitBlockLocal = new[] { waitBlock };

                using (var context = ApplicationContext.Create(application => application
                    .ConfigureForUnitTest()
                    .WithWaitBlock(waitBlockLocal[0])
                    .UseLiteServer(liteServer => liteServer
                        .AddServer<RestartableServer>())
                    .Services(services => services
                        .Advanced(advanced => advanced
                            .Register(kernel => logger)
                            .Register(kernel => state)))))
                {
                    context.Execute(nameof(LiteServerHost));
                }
            }

            Assert.AreEqual(state.NumberOfRestarts, state.Context?.FailedCount);
            logger.Received((int) state.NumberOfRestarts).LogError(Arg.Is<RestartableServer.CreateException>(x => true));
        }

        [Test]
        public void RestartableServer_Failing_ShouldRestartAlsoFails()
        {
            var logger = Substitute.For<ILogger>();
            var state = new RestartableServer.State(1);

            using (var waitBlock = new WaitBlock())
            {
                var waitBlockLocal = new[] { waitBlock };

                using (var context = ApplicationContext.Create(application => application
                    .ConfigureForUnitTest()
                    .WithWaitBlock(waitBlockLocal[0])
                    .UseLiteServer(liteServer => liteServer
                        .AddServer<FailingRestartableServer>()
                        .AddServer<RestartableServer>())
                    .Services(services => services
                        .Advanced(advanced => advanced
                            .Register(kernel => logger)
                            .Register(kernel => state)))))
                {
                    context.Execute(nameof(LiteServerHost));
                }
            }

            Assert.AreEqual(state.NumberOfRestarts, state.Context?.FailedCount);
            logger.Received(1).LogError(Arg.Is<RestartableServer.CreateException>(x => true));
            logger.Received(1).LogError(Arg.Is<FailingRestartableServer.CreateException>(x => true));
            logger.Received(1).LogError(Arg.Is<FailingRestartableServer.ShouldRestartException>(x => true));
        }

        [Test]
        public void TaskFromStateServer_TaskFromResult_NoErrors()
        {
            var logger = Substitute.For<ILogger>();
            var state = new TaskFromStateServer.State(wb =>
            {
                wb.Release();
                return Task.FromResult(true);
            });

            using (var waitBlock = new WaitBlock())
            {
                var waitBlockLocal = new[] { waitBlock };

                using (var context = ApplicationContext.Create(application => application
                    .ConfigureForUnitTest()
                    .WithWaitBlock(waitBlockLocal[0])
                    .UseLiteServer(liteServer => liteServer
                        .AddServer<TaskFromStateServer>())
                    .Services(services => services
                        .Advanced(advanced => advanced
                            .Register(kernel => logger)
                            .Register(kernel => state)))))
                {
                    context.Execute(nameof(LiteServerHost));
                }
            }
        }

        [Test]
        public void TaskFromStateServer_TaskFromException_EnsureIsLogged()
        {
            var exception = new InvalidOperationException("Some exception");
            var logger = Substitute.For<ILogger>();
            var state = new TaskFromStateServer.State(wb =>
            {
                wb.Release();
                return Task.FromException(exception);
            });

            using (var waitBlock = new WaitBlock())
            {
                var waitBlockLocal = new[] { waitBlock };

                using (var context = ApplicationContext.Create(application => application
                    .ConfigureForUnitTest()
                    .WithWaitBlock(waitBlockLocal[0])
                    .UseLiteServer(liteServer => liteServer
                        .AddServer<TaskFromStateServer>())
                    .Services(services => services
                        .Advanced(advanced => advanced
                            .Register(kernel => logger)
                            .Register(kernel => state)))))
                {
                    context.Execute(nameof(LiteServerHost));
                }
            }

            logger.Received(1).LogError(Arg.Is(exception));
        }

        [Test]
        public void TaskFromStateServer_ThrowsBeforeCreatingTask_EnsureIsLogged()
        {
            var exception = new InvalidOperationException("Some message");

            var logger = Substitute.For<ILogger>();
            var state = new TaskFromStateServer.State(wb =>
            {
                wb.Release();
                throw exception;
            });

            using (var waitBlock = new WaitBlock())
            {
                var waitBlockLocal = new[] { waitBlock };

                using (var context = ApplicationContext.Create(application => application
                    .ConfigureForUnitTest()
                    .WithWaitBlock(waitBlockLocal[0])
                    .UseLiteServer(liteServer => liteServer
                        .AddServer<TaskFromStateServer>())
                    .Services(services => services
                        .Advanced(advanced => advanced
                            .Register(kernel => logger)
                            .Register(kernel => state)))))
                {
                    context.Execute(nameof(LiteServerHost));
                }
            }

            logger.Received(1).LogError(Arg.Is(exception));
        }

        [Test]
        public void TaskFromStateServer_TaskFromCanceled_NoErrors()
        {
            var logger = Substitute.For<ILogger>();
            var state = new TaskFromStateServer.State(wb =>
            {
                wb.Release();
                return Task.FromCanceled(new CancellationToken(true));
            });

            using (var waitBlock = new WaitBlock())
            {
                var waitBlockLocal = new[] { waitBlock };

                using (var context = ApplicationContext.Create(application => application
                    .ConfigureForUnitTest()
                    .WithWaitBlock(waitBlockLocal[0])
                    .UseLiteServer(liteServer => liteServer
                        .AddServer<TaskFromStateServer>())
                    .Services(services => services
                        .Advanced(advanced => advanced
                            .Register(kernel => logger)
                            .Register(kernel => state)))))
                {
                    context.Execute(nameof(LiteServerHost));
                }
            }
        }

        public class TaskFromStateServer : IBackgroundServer
        {
            private readonly State _state;
            private readonly WaitBlock _waitBlock;

            public TaskFromStateServer(State state, WaitBlock waitBlock)
            {
                _state = state;
                _waitBlock = waitBlock;
            }

            public Task Create(BackgroundServerContext context, CancellationToken token)
            {
                return _state.TaskToReturn(_waitBlock);
            }

            public override string ToString()
            {
                return nameof(TaskFromStateServer);
            }

            public class State
            {
                public Func<WaitBlock, Task> TaskToReturn { get; }

                public State(Func<WaitBlock, Task> taskToReturn)
                {
                    TaskToReturn = taskToReturn;
                }
            }
        }

        public class RestartableServer : IBackgroundServer, IRestartable
        {
            private readonly State _state;
            private readonly WaitBlock _waitBlock;

            public RestartableServer(State state, WaitBlock waitBlock)
            {
                _state = state;
                _waitBlock = waitBlock;
            }

            public Task Create(BackgroundServerContext context, CancellationToken token)
            {
                return Task.Run(() => throw new CreateException(), token);
            }

            public bool ShouldRestart(RestartableContext context)
            {
                _state.Context = context;

                if (context.FailedCount < _state.NumberOfRestarts)
                    return true;

                _waitBlock.Release();
                return false;
            }

            public override string ToString()
            {
                return nameof(RestartableServer);
            }

            public class CreateException : Exception
            {
                public CreateException()
                    : base(nameof(CreateException))
                {
                }
            }

            public class State
            {
                public State(uint numberOfRestarts)
                {
                    NumberOfRestarts = numberOfRestarts;
                }

                public uint NumberOfRestarts { get; }
                public RestartableContext Context { get; set; }
            }
        }

        public class RestartableWorker : IBackgroundWorker, IRestartable
        {
            private readonly State _state;
            private readonly WaitBlock _waitBlock;

            public RestartableWorker(State state, WaitBlock waitBlock)
            {
                _state = state;
                _waitBlock = waitBlock;
            }

            public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
            {
                throw new WorkException();
            }
            
            public bool ShouldRestart(RestartableContext context)
            {
                _state.Context = context;

                if (context.FailedCount < _state.NumberOfRestarts)
                    return true;

                _waitBlock.Release();
                return false;
            }

            public override string ToString()
            {
                return nameof(RestartableWorker);
            }

            public class WorkException : Exception
            {
                public WorkException()
                    : base(nameof(WorkException))
                {
                }
            }

            public class State
            {
                public State(uint numberOfRestarts)
                {
                    NumberOfRestarts = numberOfRestarts;
                }

                public uint NumberOfRestarts { get; }
                public RestartableContext Context { get; set; }
            }
        }

        public class FailingRestartableServer : IBackgroundServer, IRestartable
        {
            public Task Create(BackgroundServerContext context, CancellationToken token)
            {
                return Task.Run(() => throw new CreateException(), token);
            }

            public bool ShouldRestart(RestartableContext context)
            {
                throw new ShouldRestartException();
            }

            public override string ToString()
            {
                return nameof(FailingRestartableServer);
            }

            public class CreateException : Exception
            {
            }

            public class ShouldRestartException : Exception
            {
            }
        }
        
        public class FailingWorker : IBackgroundWorker
        {
            private readonly WaitBlock _waitBlock;

            public FailingWorker(WaitBlock waitBlock)
            {
                _waitBlock = waitBlock;
            }

            public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
            {
                _waitBlock.Release();
                throw new WorkException();
            }

            public override string ToString()
            {
                return nameof(FailingWorker);
            }

            public class WorkException : Exception
            {
                public WorkException()
                    : base(nameof(WorkException))
                {
                }
            }
        }
    }
}