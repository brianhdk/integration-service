using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Exceptions;
using Vertica.Integration.Model.Tasks;

namespace Vertica.Integration.Tests.Model
{
	[TestFixture]
	public class TaskRunnerTester
	{
	    [Test]
	    public void Execute_Task_VerifyLogging()
	    {
	        var logger = Substitute.For<ILogger>();
	        var concurrentExecution = Substitute.For<IConcurrentTaskExecution>();
	        var shutdown = Substitute.For<IShutdown>();
	        var console = Substitute.For<IConsoleWriter>();
	        
	        var task = Substitute.For<ITask<object>>();

	        concurrentExecution
	            .Handle(Arg.Is(task), Arg.Any<Arguments>(), Arg.Any<TaskLog>())
	            .Returns(ConcurrentTaskExecutionResult.Continue());

	        var subject = new TaskRunner(logger, concurrentExecution, shutdown, console);

	        subject.Execute(task);

	        logger.Received().LogEntry(Arg.Any<TaskLog>());
	        task.Received().Start(Arg.Any<ITaskExecutionContext<object>>());
	    }

	    [Test]
		public void Execute_TaskWithSteps_VerifyLogging()
		{
            var logger = Substitute.For<ILogger>();
            var concurrentExecution = Substitute.For<IConcurrentTaskExecution>();
            var shutdown = Substitute.For<IShutdown>();
            var console = Substitute.For<IConsoleWriter>();

            var step1 = Substitute.For<IStep<SomeWorkItem>>();
            var step2 = Substitute.For<IStep<SomeWorkItem>>();
            var workItem = new SomeWorkItem();
		    var task = new TaskRunnerTesterTask<SomeWorkItem>(new[] {step1, step2}, workItem);

		    concurrentExecution
		        .Handle(Arg.Is(task), Arg.Any<Arguments>(), Arg.Any<TaskLog>())
		        .Returns(ConcurrentTaskExecutionResult.Continue());

		    ITaskExecutionContext<SomeWorkItem> ContextArg() => 
		        Arg.Is<ITaskExecutionContext<SomeWorkItem>>(context => context.WorkItem == workItem);

		    step1.ContinueWith(ContextArg()).Returns(Execution.Execute);
			step2.ContinueWith(ContextArg()).Returns(Execution.Execute);

			var subject = new TaskRunner(logger, concurrentExecution, shutdown, console);

			subject.Execute(task);

			logger.Received().LogEntry(Arg.Any<TaskLog>());
			logger.Received().LogEntry(Arg.Is<StepLog>(x => x.Name == step1.Name()));
			logger.Received().LogEntry(Arg.Is<StepLog>(x => x.Name == step2.Name()));

            step1.Received().Execute(ContextArg());
            step2.Received().Execute(ContextArg());

		    Assert.That(task.EndCalled, Is.True);
		}

	    [Test]
	    public void Execute_TaskThrowsAggregateException_VerifyLogging()
	    {
	        var logger = Substitute.For<ILogger>();
	        var concurrentExecution = Substitute.For<IConcurrentTaskExecution>();
	        var shutdown = Substitute.For<IShutdown>();
	        var console = Substitute.For<IConsoleWriter>();
	        var task = new ThrowingAggregateExceptionTask();

	        concurrentExecution
	            .Handle(Arg.Is(task), Arg.Any<Arguments>(), Arg.Any<TaskLog>())
	            .Returns(ConcurrentTaskExecutionResult.Continue());

	        var subject = new TaskRunner(logger, concurrentExecution, shutdown, console);

	        var thrownException = Assert.Throws<TaskExecutionFailedException>(() => subject.Execute(task));
	        var aggregateException = thrownException.InnerException as AggregateException;

	        Assert.IsNotNull(aggregateException);
	        CollectionAssert.Contains(aggregateException.InnerExceptions.Select(x => x.GetType()), typeof(InvalidOperationException));
	        CollectionAssert.Contains(aggregateException.InnerExceptions.Select(x => x.GetType()), typeof(DivideByZeroException));

	        logger.Received().LogError(Arg.Is(aggregateException));
	    }

	    [Test]
		public void Execute_FailsAtStep_VerifyLogging()
		{
            var logger = Substitute.For<ILogger>();
            var concurrentExecution = Substitute.For<IConcurrentTaskExecution>();
            var shutdown = Substitute.For<IShutdown>();
            var console = Substitute.For<IConsoleWriter>();

			var step1 = Substitute.For<IStep<SomeWorkItem>>();
			var step2 = Substitute.For<IStep<SomeWorkItem>>();
            var workItem = new SomeWorkItem();
		    var task = new TaskRunnerTesterTask<SomeWorkItem>(new[] { step1, step2 }, workItem);

			var throwingException = new DivideByZeroException("error");

		    concurrentExecution
		        .Handle(Arg.Is(task), Arg.Any<Arguments>(), Arg.Any<TaskLog>())
		        .Returns(ConcurrentTaskExecutionResult.Continue());

		    ITaskExecutionContext<SomeWorkItem> ContextArg() => 
		        Arg.Is<ITaskExecutionContext<SomeWorkItem>>(context => context.WorkItem == workItem);

		    step1.ContinueWith(ContextArg()).Returns(Execution.Execute);

			step1
                .When(x => x.Execute(ContextArg()))
				.Do(x => throw throwingException);

            var subject = new TaskRunner(logger, concurrentExecution, shutdown, console);

            var thrownException = Assert.Throws<TaskExecutionFailedException>(() => subject.Execute(task));
			Assert.That(thrownException.InnerException, Is.EqualTo(throwingException));

			logger.Received().LogEntry(Arg.Any<TaskLog>());
			logger.Received().LogEntry(Arg.Is<StepLog>(x => x.Name == step1.Name()));

            step1.Received().Execute(ContextArg());
            step2.DidNotReceive().Execute(ContextArg());

			logger.Received().LogError(Arg.Is(throwingException));

            Assert.That(task.EndCalled, Is.False);
		}

	    [Test]
	    public void Execute_ConcurrentTaskExecutionWithLock_VerifyLockIsDisposed()
	    {
	        var logger = Substitute.For<ILogger>();
	        var concurrentExecution = Substitute.For<IConcurrentTaskExecution>();
	        var shutdown = Substitute.For<IShutdown>();
	        var console = Substitute.For<IConsoleWriter>();
	        var lockAcquired = Substitute.For<IDisposable>();

	        var task = Substitute.For<ITask<object>>();

	        concurrentExecution
	            .Handle(Arg.Is(task), Arg.Any<Arguments>(), Arg.Any<TaskLog>())
	            .Returns(ConcurrentTaskExecutionResult.Continue(lockAcquired));

	        var subject = new TaskRunner(logger, concurrentExecution, shutdown, console);

	        subject.Execute(task);

	        task.Received().Start(Arg.Any<ITaskExecutionContext<object>>());
            lockAcquired.Received().Dispose();
	    }

	    [Test]
	    public void Execute_ConcurrentTaskExecutionShouldStopTask_VerifyTaskNotStarted()
	    {
	        var logger = Substitute.For<ILogger>();
	        var concurrentExecution = Substitute.For<IConcurrentTaskExecution>();
	        var shutdown = Substitute.For<IShutdown>();
	        var console = Substitute.For<IConsoleWriter>();

	        var task = Substitute.For<ITask<object>>();

	        concurrentExecution
	            .Handle(Arg.Is(task), Arg.Any<Arguments>(), Arg.Any<TaskLog>())
	            .Returns(ConcurrentTaskExecutionResult.Stop());

	        var subject = new TaskRunner(logger, concurrentExecution, shutdown, console);

	        subject.Execute(task);

	        task.DidNotReceive().Start(Arg.Any<ITaskExecutionContext<object>>());
	    }

	    [Test]
	    public void Execute_TaskIsDisabled_VerifyTaskNotStarted()
	    {
	        var logger = Substitute.For<ILogger>();
	        var concurrentExecution = Substitute.For<IConcurrentTaskExecution>();
	        var shutdown = Substitute.For<IShutdown>();
	        var console = Substitute.For<IConsoleWriter>();

	        var task = Substitute.For<ITask<object>>();
	        task.IsDisabled(Arg.Any<ITaskExecutionContext>()).Returns(true);

	        concurrentExecution
	            .Handle(Arg.Is(task), Arg.Any<Arguments>(), Arg.Any<TaskLog>())
	            .Returns(ConcurrentTaskExecutionResult.Continue());

	        var subject = new TaskRunner(logger, concurrentExecution, shutdown, console);

	        subject.Execute(task);

	        task.DidNotReceive().Start(Arg.Any<ITaskExecutionContext<object>>());
	    }

	    [Test]
	    public void Execute_ConcurrentTaskExecutionThrows_VerifyLogging()
	    {
	        var logger = Substitute.For<ILogger>();
	        var concurrentExecution = Substitute.For<IConcurrentTaskExecution>();
	        var shutdown = Substitute.For<IShutdown>();
	        var console = Substitute.For<IConsoleWriter>();
            var exceptionToBeThrown = new TaskExecutionLockNotAcquiredException();

	        var task = Substitute.For<ITask<object>>();

	        concurrentExecution
	            .Handle(Arg.Is(task), Arg.Any<Arguments>(), Arg.Any<TaskLog>())
	            .Throws(exceptionToBeThrown);

	        var subject = new TaskRunner(logger, concurrentExecution, shutdown, console);

	        var thrownException = Assert.Throws<TaskExecutionFailedException>(() => subject.Execute(task));
	        Assert.That(thrownException.InnerException, Is.EqualTo(exceptionToBeThrown));

	        logger.Received().LogEntry(Arg.Any<TaskLog>());
	        logger.Received().LogError(Arg.Is(exceptionToBeThrown));

	        task.DidNotReceive().Start(Arg.Any<ITaskExecutionContext<object>>());
	    }

		[Test]
		public void Execute_IDisposableWorkItem_TaskStartFailsAndLockIsDisposed()
		{
            var logger = Substitute.For<ILogger>();
            var concurrentExecution = Substitute.For<IConcurrentTaskExecution>();
            var shutdown = Substitute.For<IShutdown>();
            var console = Substitute.For<IConsoleWriter>();

			int disposedCount = 0;
			var step1 = Substitute.For<IStep<DisposableWorkItem>>();
			var step2 = Substitute.For<IStep<DisposableWorkItem>>();
		    var lockAcquired = Substitute.For<IDisposable>();
			var workItem = new DisposableWorkItem(() => disposedCount++);
			var exception = new InvalidOperationException();
			var task = new TaskRunnerTesterTask<DisposableWorkItem>(new[] { step1, step2 }, workItem)
				.OnStart(ctx => throw exception);

		    concurrentExecution
		        .Handle(Arg.Is(task), Arg.Any<Arguments>(), Arg.Any<TaskLog>())
		        .Returns(ConcurrentTaskExecutionResult.Continue(lockAcquired));

		    ITaskExecutionContext<DisposableWorkItem> ContextArg() => 
		        Arg.Is<ITaskExecutionContext<DisposableWorkItem>>(context => context.WorkItem == workItem);

		    step1.ContinueWith(ContextArg()).Returns(Execution.Execute);
			step2.ContinueWith(ContextArg()).Returns(Execution.Execute);

            var subject = new TaskRunner(logger, concurrentExecution, shutdown, console);

            var thrownException = Assert.Throws<TaskExecutionFailedException>(() => subject.Execute(task));

			step1.DidNotReceive().Execute(ContextArg());
			step1.DidNotReceive().Execute(ContextArg());

			Assert.That(task.EndCalled, Is.False);
			Assert.That(disposedCount, Is.EqualTo(0));
			Assert.That(thrownException.InnerException, Is.SameAs(exception));
            
            lockAcquired.Received().Dispose();
		}

		[Test]
		public void Execute_IDisposableWorkItem_TaskEndFails()
		{
            var logger = Substitute.For<ILogger>();
            var concurrentExecution = Substitute.For<IConcurrentTaskExecution>();
            var shutdown = Substitute.For<IShutdown>();
            var console = Substitute.For<IConsoleWriter>();

			int disposedCount = 0;
			var step1 = Substitute.For<IStep<DisposableWorkItem>>();
			var step2 = Substitute.For<IStep<DisposableWorkItem>>();
			var workItem = new DisposableWorkItem(() => disposedCount++);
			var exception = new InvalidOperationException();

			var task = new TaskRunnerTesterTask<DisposableWorkItem>(new[] { step1, step2 }, workItem)
                .OnEnd(ctx => throw exception);

		    concurrentExecution
		        .Handle(Arg.Is(task), Arg.Any<Arguments>(), Arg.Any<TaskLog>())
		        .Returns(ConcurrentTaskExecutionResult.Continue());

		    ITaskExecutionContext<DisposableWorkItem> ContextArg() => 
		        Arg.Is<ITaskExecutionContext<DisposableWorkItem>>(context => context.WorkItem == workItem);

		    step1.ContinueWith(ContextArg()).Returns(Execution.Execute);
			step2.ContinueWith(ContextArg()).Returns(Execution.Execute);

            var subject = new TaskRunner(logger, concurrentExecution, shutdown, console);

            var thrownException = Assert.Throws<TaskExecutionFailedException>(() => subject.Execute(task));

			step1.Received().Execute(ContextArg());
			step2.Received().Execute(ContextArg());

			Assert.That(task.EndCalled, Is.True);
			Assert.That(disposedCount, Is.EqualTo(1));
			Assert.That(thrownException.InnerException, Is.SameAs(exception));
		}
        
		[Test]
		public void Execute_IDisposableWorkItem_Step2Fails()
		{
            var logger = Substitute.For<ILogger>();
            var concurrentExecution = Substitute.For<IConcurrentTaskExecution>();
            var shutdown = Substitute.For<IShutdown>();
            var console = Substitute.For<IConsoleWriter>();

			int disposedCount = 0;
			var step1 = Substitute.For<IStep<DisposableWorkItem>>();
			var step2 = Substitute.For<IStep<DisposableWorkItem>>();
			var workItem = new DisposableWorkItem(() => disposedCount++);
			var exception = new InvalidOperationException();
			var task = new TaskRunnerTesterTask<DisposableWorkItem>(new[] { step1, step2 }, workItem);

		    concurrentExecution
		        .Handle(Arg.Is(task), Arg.Any<Arguments>(), Arg.Any<TaskLog>())
		        .Returns(ConcurrentTaskExecutionResult.Continue());

		    ITaskExecutionContext<DisposableWorkItem> ContextArg() => 
		        Arg.Is<ITaskExecutionContext<DisposableWorkItem>>(context => context.WorkItem == workItem);

		    step1.ContinueWith(ContextArg()).Returns(Execution.Execute);
			step2.ContinueWith(ContextArg()).Returns(Execution.Execute);

		    step2
				.When(x => x.Execute(ContextArg()))
				.Do(x => throw exception);

            var subject = new TaskRunner(logger, concurrentExecution, shutdown, console);

            var thrownException = Assert.Throws<TaskExecutionFailedException>(() => subject.Execute(task));

			step1.Received().Execute(ContextArg());
			step2.Received().Execute(ContextArg());

            Assert.That(task.EndCalled, Is.False);
			Assert.That(disposedCount, Is.EqualTo(1));
			Assert.That(thrownException.InnerException, Is.SameAs(exception));
		}

	    public class ThrowingAggregateExceptionTask : IntegrationTask
	    {
	        public override void StartTask(ITaskExecutionContext context)
	        {
	            var failingTaskA = System.Threading.Tasks.Task.Factory.StartNew(() => throw new InvalidOperationException());
	            var failingTaskB = System.Threading.Tasks.Task.Factory.StartNew(() => throw new DivideByZeroException());

	            System.Threading.Tasks.Task.WaitAll(failingTaskA, failingTaskB);
	        }

	        public override string Description => nameof(ThrowingAggregateExceptionTask);
	    }

		public class TaskRunnerTesterTask<TWorkItem> : IntegrationTask<TWorkItem>
	    {
	        private readonly TWorkItem _workItem;
			private Action<ITaskExecutionContext> _onStart;
			private Action<ITaskExecutionContext<TWorkItem>> _onEnd;

			public TaskRunnerTesterTask(IEnumerable<IStep<TWorkItem>> steps, TWorkItem workItem)
                : base(steps)
	        {
	            _workItem = workItem;
	        }

			public TaskRunnerTesterTask<TWorkItem> OnStart(Action<ITaskExecutionContext> onStart)
			{
				_onStart = onStart;
				return this;
			}   

	        public override TWorkItem Start(ITaskExecutionContext context)
	        {
                _onStart?.Invoke(context);

                return _workItem;
	        }

			public TaskRunnerTesterTask<TWorkItem> OnEnd(Action<ITaskExecutionContext<TWorkItem>> onEnd)
			{
				_onEnd = onEnd;

				return this;
			}

	        public override void End(ITaskExecutionContext<TWorkItem> context)
	        {
	            EndCalled = true;

	            _onEnd?.Invoke(context);
            }

			public bool EndCalled { get; set; }

	        public override string Description => string.Empty;
	    }

	    public class SomeWorkItem
		{
		}

		public class DisposableWorkItem : IDisposable
		{
			private readonly Action _onDisposed;

			public DisposableWorkItem(Action onDisposed)
			{
				if (onDisposed == null) throw new ArgumentNullException(nameof(onDisposed));

				_onDisposed = onDisposed;
			}

			public void Dispose()
			{
				_onDisposed();
			}
		}
	}
}