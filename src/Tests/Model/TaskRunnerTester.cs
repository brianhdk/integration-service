using System;
using System.IO;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Exceptions;

namespace Vertica.Integration.Tests.Model
{
	[TestFixture]
	public class TaskRunnerTester
	{
		[Test]
		public void Execute_TaskWithSteps_VerifyLogging()
		{
			var logger = Substitute.For<ILogger>();
			TextWriter outputter = TextWriter.Null;

			var task = Substitute.For<ITask<SomeWorkItem>>();
			var step1 = Substitute.For<IStep<SomeWorkItem>>();
			var step2 = Substitute.For<IStep<SomeWorkItem>>();

			var workItem = new SomeWorkItem();

            task.Start(Arg.Any<ITaskExecutionContext>()).ReturnsForAnyArgs(workItem);
			task.Steps.Returns(new[] { step1, step2 });

			step1.ContinueWith(workItem).Returns(Execution.Execute);
			step2.ContinueWith(workItem).Returns(Execution.Execute);

			var subject = new TaskRunner(logger, outputter);

			subject.Execute(task);

			logger.Received().LogEntry(Arg.Any<TaskLog>());
			logger.Received().LogEntry(Arg.Is<StepLog>(x => x.StepName == TaskRunner.GetStepName(step1)));
			logger.Received().LogEntry(Arg.Is<StepLog>(x => x.StepName == TaskRunner.GetStepName(step2)));

            step1.Received().Execute(workItem, Arg.Any<ITaskExecutionContext>());
            step2.Received().Execute(workItem, Arg.Any<ITaskExecutionContext>());

            task.Received().End(workItem, Arg.Any<ITaskExecutionContext>());
		}

		[Test]
		public void Execute_FailsAtStep_VerifyLogging()
		{
			var logger = Substitute.For<ILogger>();
			TextWriter outputter = TextWriter.Null;

			var task = Substitute.For<ITask<SomeWorkItem>>();
			var step1 = Substitute.For<IStep<SomeWorkItem>>();
			var step2 = Substitute.For<IStep<SomeWorkItem>>();
			var throwingException = new DivideByZeroException("error");

			var workItem = new SomeWorkItem();

            task.Start(Arg.Any<ITaskExecutionContext>()).Returns(workItem);
			task.Steps.Returns(new[] { step1, step2 });

			step1.ContinueWith(workItem).Returns(Execution.Execute);

			step1
                .When(x => x.Execute(workItem, Arg.Any<ITaskExecutionContext>()))
				.Do(x => { throw throwingException; });

			var subject = new TaskRunner(logger, outputter);

			var thrownException = Assert.Throws<TaskExecutionFailedException>(() => subject.Execute(task));
			Assert.That(thrownException.InnerException, Is.EqualTo(throwingException));

			logger.Received().LogEntry(Arg.Any<TaskLog>());
			logger.Received().LogEntry(Arg.Is<StepLog>(x => x.StepName == TaskRunner.GetStepName(step1)));

            step1.Received().Execute(workItem, Arg.Any<ITaskExecutionContext>());
            step2.DidNotReceive().Execute(workItem, Arg.Any<ITaskExecutionContext>());

			logger.Received().LogError(Arg.Is(throwingException));

            task.DidNotReceive().End(workItem, Arg.Any<ITaskExecutionContext>());
		}

		public class SomeWorkItem
		{
		}
	}
}