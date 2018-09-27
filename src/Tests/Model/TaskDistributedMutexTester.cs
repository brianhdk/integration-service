using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Threading.DistributedMutex;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Exceptions;
using Vertica.Integration.Model.Tasks;
using Vertica.Utilities;
using Vertica.Integration.Tests.Infrastructure.Testing;

namespace Vertica.Integration.Tests.Model
{
    [TestFixture]
    public class TaskDistributedMutexTester
    {
        [Test]
        public void RunSameTaskInParallel_Verify_OneTaskThrows()
        {
            var distributedMutex = new DistributedMutexStub();
            var cancellationTokenSource = new CancellationTokenSource();
            var synchronizationContext = new SynchronizationContext(cancellationTokenSource);

            using (var context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Register(kernel => synchronizationContext)
                        .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings()
                            .Set("ConcurrentTaskExecution.PreventConcurrentTaskExecutionOnAllTasks", "true"))
                        .Register<IDistributedMutex>(kernel => distributedMutex)))
                .Tasks(tasks => tasks
                    .Task<SomeTask>())))
            {
                var runner = context.Resolve<ITaskRunner>();
                var factory = context.Resolve<ITaskFactory>();

                var taskA = System.Threading.Tasks.Task.Factory.StartNew(() => 
                    runner.Execute(factory.Get<SomeTask>()), cancellationTokenSource.Token);

                var taskB = System.Threading.Tasks.Task.Factory.StartNew(() =>
                    runner.Execute(factory.Get<SomeTask>()), cancellationTokenSource.Token);

                cancellationTokenSource.CancelAfter(100);

                try
                {
                    System.Threading.Tasks.Task.WaitAll(taskA, taskB);
                }
                catch (AggregateException)
                {
                }

                var tasks = new[] {taskA, taskB};
                var failedTasks = tasks.Where(x => x.Exception != null).ToArray();

                Assert.That(failedTasks.Length, Is.EqualTo(1));
                Assert.That(((Exception) failedTasks[0].Exception).AggregateMessages(), Does.Contain("Unable to acquire lock 'SomeTask'"));

                var completedTasks = tasks.Where(x => x.Status == TaskStatus.RanToCompletion).ToArray();

                Assert.That(completedTasks.Length, Is.EqualTo(1));
            }
        }

        [Test]
        public void Task_With_PreventConcurrentTaskExecutionAttribute_Verify_InteractsWithDistributedMutex()
        {
            var distributedMutex = Substitute.For<IDistributedMutex>();

            using (var context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings()
                            .Set("ConcurrentTaskExecution.PreventConcurrentTaskExecutionOnAllTasks", "false"))
                        .Register(kernel => distributedMutex)))
                .Tasks(tasks => tasks
                    .Task<SyncOnlyWithAttributeTask>())))
            {
                var runner = context.Resolve<ITaskRunner>();
                var factory = context.Resolve<ITaskFactory>();

                runner.Execute(factory.Get<SyncOnlyWithAttributeTask>());

                distributedMutex
                    .Received(1)
                    .Enter(Arg.Is<DistributedMutexContext>(x => x.Name == nameof(SyncOnlyWithAttributeTask)));
            }
        }

        [Test]
        public void Task_With_PreventConcurrentTaskExecutionAttribute_RuntimeEvaluator_Verify_DoesNotInteractWithDistributedMutex()
        {
            var distributedMutex = Substitute.For<IDistributedMutex>();

            using (var context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings()
                            .Set("ConcurrentTaskExecution.PreventConcurrentTaskExecutionOnAllTasks", "false"))
                        .Register(kernel => distributedMutex)))
                .Tasks(tasks => tasks
                    .Task<SyncOnlyWithAttributeAndRuntimeEvaluatorTask>()
                    .ConcurrentTaskExecution(concurrentTaskExecution => concurrentTaskExecution
                        .AddRuntimeEvaluator<CustomRuntimeEvaluator>()))))
            {
                var runner = context.Resolve<ITaskRunner>();
                var factory = context.Resolve<ITaskFactory>();

                runner.Execute(factory.Get<SyncOnlyWithAttributeAndRuntimeEvaluatorTask>());

                distributedMutex
                    .DidNotReceiveWithAnyArgs()
                    .Enter(Arg.Any<DistributedMutexContext>());
            }
        }

        [Test]
        public void Task_With_PreventConcurrentTaskExecutionAttribute_CustomLockName_Verify_InteractsWithDistributedMutex()
        {
            var distributedMutex = Substitute.For<IDistributedMutex>();

            using (var context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings()
                            .Set("ConcurrentTaskExecution.PreventConcurrentTaskExecutionOnAllTasks", "false"))
                        .Register(kernel => distributedMutex)))
                .Tasks(tasks => tasks
                    .Task<SyncOnlyWithAttributeAndCustomLockNameAndDescriptionTask>()
                    .ConcurrentTaskExecution(concurrentTaskExecution => concurrentTaskExecution
                        .AddCustomLockName<CustomLockName>()
                        .AddCustomLockDescription<CustomLockDescription>()))))
            {
                var runner = context.Resolve<ITaskRunner>();
                var factory = context.Resolve<ITaskFactory>();

                runner.Execute(
                    factory.Get<SyncOnlyWithAttributeAndCustomLockNameAndDescriptionTask>(), 
                    new Arguments(
                        new KeyValuePair<string, string>("LockName", "MyLockName"),
                        new KeyValuePair<string, string>("LockDescription", "MyLockDescription")));
                
                distributedMutex
                    .Received(1)
                    .Enter(Arg.Is<DistributedMutexContext>(x => 
                        x.Name == "MyLockName" &&
                        x.Description == "MyLockDescription"));
            }
        }

        [Test]
        public void Task_With_PreventConcurrentTaskExecutionAttribute_ReturnNullExceptionHandler_Verify_InteractsWithDistributedMutex()
        {
            var distributedMutex = new DistributedMutexStub();

            using (var context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings()
                            .Set("ConcurrentTaskExecution.PreventConcurrentTaskExecutionOnAllTasks", "false"))
                        .Register<IDistributedMutex>(kernel => distributedMutex)))
                .Tasks(tasks => tasks
                    .Task<ReturnNullOnSyncExceptionTask>()
                    .ConcurrentTaskExecution(concurrentTaskExecution => concurrentTaskExecution
                        .AddExceptionHandler<ReturnNullExceptionHandler>()))))
            {
                var runner = context.Resolve<ITaskRunner>();
                var factory = context.Resolve<ITaskFactory>();

                // Create an existing lock based on the actual task
                distributedMutex.SimulateLockFromTask<ReturnNullOnSyncExceptionTask>();

                ITask task = factory.Get<ReturnNullOnSyncExceptionTask>();

                runner.Execute(task);
            }
        }

        [Test]
        public void Task_With_PreventConcurrentTaskExecutionAttribute_ThrowsDifferentExceptionExceptionHandler_Verify_InteractsWithDistributedMutex()
        {
            var distributedMutex = new DistributedMutexStub();

            using (var context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings()
                            .Set("ConcurrentTaskExecution.PreventConcurrentTaskExecutionOnAllTasks", "false"))
                        .Register<IDistributedMutex>(kernel => distributedMutex)))
                .Tasks(tasks => tasks
                    .Task<ThrowOnSyncExceptionTask>()
                    .ConcurrentTaskExecution(concurrentTaskExecution => concurrentTaskExecution
                        .AddExceptionHandler<ThrowingExceptionHandler>()))))
            {
                var runner = context.Resolve<ITaskRunner>();
                var factory = context.Resolve<ITaskFactory>();

                // Create an existing lock based on the actual task
                distributedMutex.SimulateLockFromTask<ThrowOnSyncExceptionTask>();

                ITask task = factory.Get<ThrowOnSyncExceptionTask>();

                var exception = Assert.Throws<TaskExecutionFailedException>(() => runner.Execute(task));

                Assert.IsNotNull(exception.InnerException);
                Assert.IsInstanceOf<ThrowingExceptionHandler.DifferentException>(exception.InnerException);

                Assert.IsNotNull(exception.InnerException.InnerException);
                Assert.IsInstanceOf<DistributedMutexStub.IsLockedException>(exception.InnerException.InnerException);
            }
        }

        [Test]
        public void Task_With_PreventConcurrentTaskExecutionAttribute_UnableToAcquireLock_Verify_InteractsWithDistributedMutex()
        {
            var distributedMutex = new DistributedMutexStub();

            using (var context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings()
                            .Set("ConcurrentTaskExecution.PreventConcurrentTaskExecutionOnAllTasks", "false"))
                        .Register<IDistributedMutex>(kernel => distributedMutex)))
                .Tasks(tasks => tasks
                    .Task<ThrowIfStartsTask>())))
            {
                var runner = context.Resolve<ITaskRunner>();
                var factory = context.Resolve<ITaskFactory>();

                // Create an existing lock based on the actual task
                distributedMutex.SimulateLockFromTask<ThrowIfStartsTask>();

                ITask task = factory.Get<ThrowIfStartsTask>();

                var exception = Assert.Throws<TaskExecutionFailedException>(() => runner.Execute(task));

                Assert.IsNotNull(exception.InnerException);
                Assert.IsInstanceOf<TaskExecutionLockNotAcquiredException>(exception.InnerException);

                Assert.IsNotNull(exception.InnerException.InnerException);
                Assert.IsInstanceOf<DistributedMutexStub.IsLockedException>(exception.InnerException.InnerException);
            }
        }
        public class DistributedMutexStub : IDistributedMutex
        {
            private readonly ConcurrentDictionary<string, DistributedMutexContext> _locks;

            public DistributedMutexStub()
            {
                _locks = new ConcurrentDictionary<string, DistributedMutexContext>();
            }

            public IDisposable Enter(DistributedMutexContext context)
            {
                if (_locks.ContainsKey(context.Name))
                    throw new IsLockedException();

                if (_locks.TryAdd(context.Name, context))
                {
                    return new DisposableAction(() =>
                    {
                        DistributedMutexContext dummy;
                        _locks.TryRemove(context.Name, out dummy);
                    });
                }

                throw new InvalidOperationException($"Unable to add lock for '{context.Name}.");
            }

            public void SimulateLockFromTask<TTask>() where TTask : ITask
            {
                SimulateLock(typeof(TTask).Name);
            }

            public void SimulateLock(string key)
            {
                _locks.AddOrUpdate(key, (DistributedMutexContext) null, (existingKey, existingValue) => existingValue);
            }

            public class IsLockedException : Exception
            {
            }
        }

        public class SynchronizationContext
        {
            private readonly CancellationTokenSource _source;

            public SynchronizationContext(CancellationTokenSource source)
            {
                if (source == null) throw new ArgumentNullException(nameof(source));

                _source = source;
            }

            public CancellationToken Token => _source.Token;
        }

        public class SomeTask : IntegrationTask
        {
            private readonly SynchronizationContext _context;

            public SomeTask(SynchronizationContext context)
            {
                _context = context;
            }

            public override void StartTask(ITaskExecutionContext context)
            {
                _context.Token.WaitHandle.WaitOne();
            }

            public override string Description => "TBD";
        }

        [PreventConcurrentTaskExecution]
        public class SyncOnlyWithAttributeTask : IntegrationTask
        {
            public override void StartTask(ITaskExecutionContext context)
            {
            }

            public override string Description => "TBD";
        }

        public class CustomRuntimeEvaluator : IPreventConcurrentTaskExecutionRuntimeEvaluator
        {
            public bool Disabled(ITask currentTask, Arguments arguments)
            {
                return true;
            }
        }

        public class CustomLockName : IPreventConcurrentTaskExecutionCustomLockName
        {
            public string GetLockName(ITask currentTask, Arguments arguments)
            {
                return arguments["LockName"];
            }
        }

        public class CustomLockDescription : IPreventConcurrentTaskExecutionCustomLockDescription
        {
            public string GetLockDescription(ITask currentTask, Arguments arguments, string currentDescription)
            {
                return arguments["LockDescription"];
            }
        }

        public class ReturnNullExceptionHandler : IPreventConcurrentTaskExecutionExceptionHandler
        {
            public Exception OnException(ITask currentTask, TaskLog log, Arguments arguments, Exception exception)
            {
                return null;
            }
        }

        public class ThrowingExceptionHandler : IPreventConcurrentTaskExecutionExceptionHandler
        {
            public Exception OnException(ITask currentTask, TaskLog log, Arguments arguments, Exception exception)
            {
                throw new DifferentException("Message", exception);
            }

            [Serializable]
            public class DifferentException : Exception
            {
                public DifferentException(string message, Exception inner)
                    : base(message, inner)
                {
                }
            }
        }

        [PreventConcurrentTaskExecution(RuntimeEvaluator = typeof(CustomRuntimeEvaluator))]
        public class SyncOnlyWithAttributeAndRuntimeEvaluatorTask : IntegrationTask
        {
            public override void StartTask(ITaskExecutionContext context)
            {
            }

            public override string Description => "TBD";
        }

        [PreventConcurrentTaskExecution(CustomLockName = typeof(CustomLockName), CustomLockDescription = typeof(CustomLockDescription))]
        public class SyncOnlyWithAttributeAndCustomLockNameAndDescriptionTask : IntegrationTask
        {
            public override void StartTask(ITaskExecutionContext context)
            {
            }

            public override string Description => "TBD";
        }

        [PreventConcurrentTaskExecution(ExceptionHandler = typeof(ReturnNullExceptionHandler))]
        public class ReturnNullOnSyncExceptionTask : IntegrationTask
        {
            public override void StartTask(ITaskExecutionContext context)
            {
                Assert.Fail("This task should never be started.");
            }

            public override string Description => "TBD";
        }

        [PreventConcurrentTaskExecution(ExceptionHandler = typeof(ThrowingExceptionHandler))]
        public class ThrowOnSyncExceptionTask : IntegrationTask
        {
            public override void StartTask(ITaskExecutionContext context)
            {
                Assert.Fail("This task should never be started.");
            }

            public override string Description => "TBD";
        }

        [PreventConcurrentTaskExecution]
        public class ThrowIfStartsTask : IntegrationTask
        {
            public override void StartTask(ITaskExecutionContext context)
            {
                Assert.Fail("This task should never be started.");
            }

            public override string Description => "TBD";
        }
    }
}
