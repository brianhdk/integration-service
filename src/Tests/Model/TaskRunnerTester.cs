using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
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
	    private ILogger _logger;
	    private IConcurrentTaskExecution _concurrentExecution;
	    private IShutdown _shutdown;
	    private IConsoleWriter _console;

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

			step1.ContinueWith(workItem, Arg.Any<ITaskExecutionContext>()).Returns(Execution.Execute);
			step2.ContinueWith(workItem, Arg.Any<ITaskExecutionContext>()).Returns(Execution.Execute);

			var subject = new TaskRunner(logger, concurrentExecution, shutdown, console);

			subject.Execute(task);

			logger.Received().LogEntry(Arg.Any<TaskLog>());
			logger.Received().LogEntry(Arg.Is<StepLog>(x => x.Name == step1.Name()));
			logger.Received().LogEntry(Arg.Is<StepLog>(x => x.Name == step2.Name()));

            step1.Received().Execute(workItem, Arg.Any<ITaskExecutionContext>());
            step2.Received().Execute(workItem, Arg.Any<ITaskExecutionContext>());

		    Assert.That(task.EndCalled, Is.True);
		}

	    [Test]
	    public void Execute_TaskThrowsAggregateException_VerifyLogging()
	    {
	        TaskRunner subject = InitializeSubject();

	        var task = new ThrowingAggregateExceptionTask();

	        var thrownException = Assert.Throws<TaskExecutionFailedException>(() => subject.Execute(task));
	        var aggregateException = thrownException.InnerException as AggregateException;

	        Assert.IsNotNull(aggregateException);
	        CollectionAssert.Contains(aggregateException.InnerExceptions.Select(x => x.GetType()), typeof(InvalidOperationException));
	        CollectionAssert.Contains(aggregateException.InnerExceptions.Select(x => x.GetType()), typeof(DivideByZeroException));

	        _logger.Received().LogError(Arg.Is(aggregateException));
	    }

        private TaskRunner InitializeSubject()
	    {
	        _logger = Substitute.For<ILogger>();
	        _concurrentExecution = Substitute.For<IConcurrentTaskExecution>();
	        _shutdown = Substitute.For<IShutdown>();
	        _console = Substitute.For<IConsoleWriter>();

	        var subject = new TaskRunner(_logger, _concurrentExecution, _shutdown, _console);

	        return subject;
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
            
			step1.ContinueWith(workItem, Arg.Any<ITaskExecutionContext>()).Returns(Execution.Execute);

			step1
                .When(x => x.Execute(workItem, Arg.Any<ITaskExecutionContext>()))
				.Do(x => throw throwingException);

            var subject = new TaskRunner(logger, concurrentExecution, shutdown, console);

            var thrownException = Assert.Throws<TaskExecutionFailedException>(() => subject.Execute(task));
			Assert.That(thrownException.InnerException, Is.EqualTo(throwingException));

			logger.Received().LogEntry(Arg.Any<TaskLog>());
			logger.Received().LogEntry(Arg.Is<StepLog>(x => x.Name == step1.Name()));

            step1.Received().Execute(workItem, Arg.Any<ITaskExecutionContext>());
            step2.DidNotReceive().Execute(workItem, Arg.Any<ITaskExecutionContext>());

			logger.Received().LogError(Arg.Is(throwingException));

            Assert.That(task.EndCalled, Is.False);
		}

		[Test]
		public void Execute_IDisposableWorkItem_TaskStartFails()
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
				.OnStart(ctx => throw exception);

			step1.ContinueWith(workItem, Arg.Any<ITaskExecutionContext>()).Returns(Execution.Execute);
			step2.ContinueWith(workItem, Arg.Any<ITaskExecutionContext>()).Returns(Execution.Execute);

            var subject = new TaskRunner(logger, concurrentExecution, shutdown, console);

            var thrownException = Assert.Throws<TaskExecutionFailedException>(() => subject.Execute(task));

			step1.DidNotReceive().Execute(workItem, Arg.Any<ITaskExecutionContext>());
			step1.DidNotReceive().Execute(workItem, Arg.Any<ITaskExecutionContext>());

			Assert.That(task.EndCalled, Is.False);
			Assert.That(disposedCount, Is.EqualTo(0));
			Assert.That(thrownException.InnerException, Is.SameAs(exception));
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
				.OnEnd((wi, ctx) => throw exception);

			step1.ContinueWith(workItem, Arg.Any<ITaskExecutionContext>()).Returns(Execution.Execute);
			step2.ContinueWith(workItem, Arg.Any<ITaskExecutionContext>()).Returns(Execution.Execute);

            var subject = new TaskRunner(logger, concurrentExecution, shutdown, console);

            var thrownException = Assert.Throws<TaskExecutionFailedException>(() => subject.Execute(task));

			step1.Received().Execute(workItem, Arg.Any<ITaskExecutionContext>());
			step2.Received().Execute(workItem, Arg.Any<ITaskExecutionContext>());

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

			step1.ContinueWith(workItem, Arg.Any<ITaskExecutionContext>()).Returns(Execution.Execute);
			step2.ContinueWith(workItem, Arg.Any<ITaskExecutionContext>()).Returns(Execution.Execute);

			step2
				.When(x => x.Execute(workItem, Arg.Any<ITaskExecutionContext>()))
				.Do(x => throw exception);

            var subject = new TaskRunner(logger, concurrentExecution, shutdown, console);

            var thrownException = Assert.Throws<TaskExecutionFailedException>(() => subject.Execute(task));

			step1.Received().Execute(workItem, Arg.Any<ITaskExecutionContext>());
			step2.Received().Execute(workItem, Arg.Any<ITaskExecutionContext>());

			Assert.That(task.EndCalled, Is.False);
			Assert.That(disposedCount, Is.EqualTo(1));
			Assert.That(thrownException.InnerException, Is.SameAs(exception));
		}

	    public class ThrowingAggregateExceptionTask : Task
	    {
	        public override void StartTask(ITaskExecutionContext context)
	        {
	            var failingTaskA = System.Threading.Tasks.Task.Factory.StartNew(() => throw new InvalidOperationException());
	            var failingTaskB = System.Threading.Tasks.Task.Factory.StartNew(() => throw new DivideByZeroException());

	            System.Threading.Tasks.Task.WaitAll(failingTaskA, failingTaskB);
	        }

	        public override string Description => nameof(ThrowingAggregateExceptionTask);
	    }

		public class TaskRunnerTesterTask<TWorkItem> : Task<TWorkItem>
	    {
	        private readonly TWorkItem _workItem;
			private Action<ITaskExecutionContext> _onStart;
			private Action<TWorkItem, ITaskExecutionContext> _onEnd;

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

			public TaskRunnerTesterTask<TWorkItem> OnEnd(Action<TWorkItem, ITaskExecutionContext> onEnd)
			{
				_onEnd = onEnd;

				return this;
			}

			public override void End(TWorkItem workItem, ITaskExecutionContext context)
			{
	            EndCalled = true;

			    _onEnd?.Invoke(workItem, context);
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