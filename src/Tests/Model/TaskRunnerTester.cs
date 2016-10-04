using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Exceptions;
using Vertica.Integration.Model.Tasks;

namespace Vertica.Integration.Tests.Model
{
	[TestFixture]
	[SuppressMessage("ReSharper", "UseNullPropagation")]
	public class TaskRunnerTester
	{
		[Test]
		public void Execute_TaskWithSteps_VerifyLogging()
		{
			var logger = Substitute.For<ILogger>();
            var concurrentExecution = Substitute.For<IConcurrentTaskExecution>();
            TextWriter outputter = TextWriter.Null;

            var step1 = Substitute.For<IStep<SomeWorkItem>>();
            var step2 = Substitute.For<IStep<SomeWorkItem>>();
            var workItem = new SomeWorkItem();
		    var task = new TaskRunnerTesterTask<SomeWorkItem>(new[] {step1, step2}, workItem);

			step1.ContinueWith(workItem).Returns(Execution.Execute);
			step2.ContinueWith(workItem).Returns(Execution.Execute);

			var subject = new TaskRunner(logger, concurrentExecution, outputter);

			subject.Execute(task);

			logger.Received().LogEntry(Arg.Any<TaskLog>());
			logger.Received().LogEntry(Arg.Is<StepLog>(x => x.Name == step1.Name()));
			logger.Received().LogEntry(Arg.Is<StepLog>(x => x.Name == step2.Name()));

            step1.Received().Execute(workItem, Arg.Any<ITaskExecutionContext>());
            step2.Received().Execute(workItem, Arg.Any<ITaskExecutionContext>());

		    Assert.That(task.EndCalled, Is.True);
		}

		[Test]
		public void Execute_FailsAtStep_VerifyLogging()
		{
			var logger = Substitute.For<ILogger>();
            var concurrentExecution = Substitute.For<IConcurrentTaskExecution>();
            TextWriter outputter = TextWriter.Null;

			var step1 = Substitute.For<IStep<SomeWorkItem>>();
			var step2 = Substitute.For<IStep<SomeWorkItem>>();
            var workItem = new SomeWorkItem();
		    var task = new TaskRunnerTesterTask<SomeWorkItem>(new[] { step1, step2 }, workItem);

			var throwingException = new DivideByZeroException("error");
            
			step1.ContinueWith(workItem).Returns(Execution.Execute);

			step1
                .When(x => x.Execute(workItem, Arg.Any<ITaskExecutionContext>()))
				.Do(x => { throw throwingException; });

			var subject = new TaskRunner(logger, concurrentExecution, outputter);

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
            TextWriter outputter = TextWriter.Null;

			int disposedCount = 0;
			var step1 = Substitute.For<IStep<DisposableWorkItem>>();
			var step2 = Substitute.For<IStep<DisposableWorkItem>>();
			var workItem = new DisposableWorkItem(() => disposedCount++);
			var exception = new InvalidOperationException();
			var task = new TaskRunnerTesterTask<DisposableWorkItem>(new[] { step1, step2 }, workItem)
				.OnStart(ctx => { throw exception; });

			step1.ContinueWith(workItem).Returns(Execution.Execute);
			step2.ContinueWith(workItem).Returns(Execution.Execute);

			var subject = new TaskRunner(logger, concurrentExecution, outputter);
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
            TextWriter outputter = TextWriter.Null;

			int disposedCount = 0;
			var step1 = Substitute.For<IStep<DisposableWorkItem>>();
			var step2 = Substitute.For<IStep<DisposableWorkItem>>();
			var workItem = new DisposableWorkItem(() => disposedCount++);
			var exception = new InvalidOperationException();
			var task = new TaskRunnerTesterTask<DisposableWorkItem>(new[] { step1, step2 }, workItem)
				.OnEnd((wi, ctx) => { throw exception; });

			step1.ContinueWith(workItem).Returns(Execution.Execute);
			step2.ContinueWith(workItem).Returns(Execution.Execute);

			var subject = new TaskRunner(logger, concurrentExecution, outputter);
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
            TextWriter outputter = TextWriter.Null;

			int disposedCount = 0;
			var step1 = Substitute.For<IStep<DisposableWorkItem>>();
			var step2 = Substitute.For<IStep<DisposableWorkItem>>();
			var workItem = new DisposableWorkItem(() => disposedCount++);
			var exception = new InvalidOperationException();
			var task = new TaskRunnerTesterTask<DisposableWorkItem>(new[] { step1, step2 }, workItem);

			step1.ContinueWith(workItem).Returns(Execution.Execute);
			step2.ContinueWith(workItem).Returns(Execution.Execute);

			step2
				.When(x => x.Execute(workItem, Arg.Any<ITaskExecutionContext>()))
				.Do(x => { throw exception; });

			var subject = new TaskRunner(logger, concurrentExecution, outputter);

			var thrownException = Assert.Throws<TaskExecutionFailedException>(() => subject.Execute(task));

			step1.Received().Execute(workItem, Arg.Any<ITaskExecutionContext>());
			step2.Received().Execute(workItem, Arg.Any<ITaskExecutionContext>());

			Assert.That(task.EndCalled, Is.False);
			Assert.That(disposedCount, Is.EqualTo(1));
			Assert.That(thrownException.InnerException, Is.SameAs(exception));
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

				if (_onEnd != null)
					_onEnd(workItem, context);
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