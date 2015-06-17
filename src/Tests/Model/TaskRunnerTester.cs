using System;
using System.Collections.Generic;
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

            var step1 = Substitute.For<IStep<SomeWorkItem>>();
            var step2 = Substitute.For<IStep<SomeWorkItem>>();
            var workItem = new SomeWorkItem();
		    var task = new TaskRunnerTesterTask(new[] {step1, step2}, workItem);

			step1.ContinueWith(workItem).Returns(Execution.Execute);
			step2.ContinueWith(workItem).Returns(Execution.Execute);

			var subject = new TaskRunner(logger, outputter);

			subject.Execute(task);

			logger.Received().LogEntry(Arg.Any<TaskLog>());
			logger.Received().LogEntry(Arg.Is<StepLog>(x => x.Name == TaskRunner.GetStepName(step1)));
			logger.Received().LogEntry(Arg.Is<StepLog>(x => x.Name == TaskRunner.GetStepName(step2)));

            step1.Received().Execute(workItem, Arg.Any<ITaskExecutionContext>());
            step2.Received().Execute(workItem, Arg.Any<ITaskExecutionContext>());

		    Assert.That(task.EndCalled, Is.True);
		}

		[Test]
		public void Execute_FailsAtStep_VerifyLogging()
		{
			var logger = Substitute.For<ILogger>();
			TextWriter outputter = TextWriter.Null;

			var step1 = Substitute.For<IStep<SomeWorkItem>>();
			var step2 = Substitute.For<IStep<SomeWorkItem>>();
            var workItem = new SomeWorkItem();
		    var task = new TaskRunnerTesterTask(new[] { step1, step2 }, workItem);

			var throwingException = new DivideByZeroException("error");
            
			step1.ContinueWith(workItem).Returns(Execution.Execute);

			step1
                .When(x => x.Execute(workItem, Arg.Any<ITaskExecutionContext>()))
				.Do(x => { throw throwingException; });

			var subject = new TaskRunner(logger, outputter);

			var thrownException = Assert.Throws<TaskExecutionFailedException>(() => subject.Execute(task));
			Assert.That(thrownException.InnerException, Is.EqualTo(throwingException));

			logger.Received().LogEntry(Arg.Any<TaskLog>());
			logger.Received().LogEntry(Arg.Is<StepLog>(x => x.Name == TaskRunner.GetStepName(step1)));

            step1.Received().Execute(workItem, Arg.Any<ITaskExecutionContext>());
            step2.DidNotReceive().Execute(workItem, Arg.Any<ITaskExecutionContext>());

			logger.Received().LogError(Arg.Is(throwingException));

            Assert.That(task.EndCalled, Is.False);
		}

	    public class TaskRunnerTesterTask : Task<SomeWorkItem>
	    {
	        private readonly SomeWorkItem _workItem;

	        public TaskRunnerTesterTask(IEnumerable<IStep<SomeWorkItem>> steps, SomeWorkItem workItem)
                : base(steps)
	        {
	            _workItem = workItem;
	        }

	        public override SomeWorkItem Start(ITaskExecutionContext context)
	        {
	            return _workItem;
	        }

	        public override void End(SomeWorkItem workItem, ITaskExecutionContext context)
	        {
	            EndCalled = true;
	        }

	        public bool EndCalled { get; set; }

	        public override string Description
	        {
	            get { return String.Empty; }
	        }
	    }

	    public class SomeWorkItem
		{
		}
	}
}