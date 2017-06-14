using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Threading.DistributedMutex;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Tasks;
using Vertica.Integration.Tests.Infrastructure;
using Vertica.Utilities;
using Task = Vertica.Integration.Model.Task;

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
                Assert.That(failedTasks[0].Exception.AggregateMessages(), Does.Contain("Unable to acquire lock 'SomeTask'"));

                var completedTasks = tasks.Where(x => x.Status == TaskStatus.RanToCompletion).ToArray();

                Assert.That(completedTasks.Length, Is.EqualTo(1));
            }
        }

        [Test]
        public void Task_With_PreventConcurrentExecutionAttribute_Verify_InteractsWithDistributedMutex()
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
        public void Task_With_PreventConcurrentExecutionAttribute_RuntimeEvaluator_Verify_DoesNotInteractWithDistributedMutex()
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
        public void Task_With_PreventConcurrentExecutionAttribute_CustomLockName_Verify_InteractsWithDistributedMutex()
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
                    .Task<SyncOnlyWithAttributeAndCustomLockNameTask>()
                    .ConcurrentTaskExecution(concurrentTaskExecution => concurrentTaskExecution
                        .AddCustomLockName<CustomLockName>()))))
            {
                var runner = context.Resolve<ITaskRunner>();
                var factory = context.Resolve<ITaskFactory>();

                runner.Execute(
                    factory.Get<SyncOnlyWithAttributeAndCustomLockNameTask>(), 
                    new Arguments(new KeyValuePair<string, string>("LockName", "MyLockName")));
                
                distributedMutex
                    .Received(1)
                    .Enter(Arg.Is<DistributedMutexContext>(x => x.Name == "MyLockName"));
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
                    throw new InvalidOperationException($"{context.Name} is locked.");

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

        public class SomeTask : Task
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
        public class SyncOnlyWithAttributeTask : Task
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

        [PreventConcurrentTaskExecution(RuntimeEvaluator = typeof(CustomRuntimeEvaluator))]
        public class SyncOnlyWithAttributeAndRuntimeEvaluatorTask : Task
        {
            public override void StartTask(ITaskExecutionContext context)
            {
            }

            public override string Description => "TBD";
        }

        [PreventConcurrentTaskExecution(CustomLockName = typeof(CustomLockName))]
        public class SyncOnlyWithAttributeAndCustomLockNameTask : Task
        {
            public override void StartTask(ITaskExecutionContext context)
            {
            }

            public override string Description => "TBD";
        }
    }
}
