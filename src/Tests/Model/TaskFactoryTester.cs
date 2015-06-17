using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Logging.Loggers;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Exceptions;

namespace Vertica.Integration.Tests.Model
{
    [TestFixture(Category = "Integration")]
    public class TaskFactoryTester
    {
        [Test]
        public void GetByName_Verify_SingletonBehaviour()
        {
            ITaskFactory subject;
            using (CreateSubject(out subject, tasks => AddFromTestAssembly(tasks)))
            {
                ITask task1 = subject.GetByName("TestTask");
                ITask task2 = subject.GetByName("TestTask");

                Assert.That(task1, Is.EqualTo(task2));
            }
        }

        [Test]
        public void GetByName_Verify_NameIsNotCaseSensitive()
        {
            ITaskFactory subject;
            using (CreateSubject(out subject, tasks => AddFromTestAssembly(tasks)))
            {
                ITask task1 = subject.GetByName("testtask");

                Assert.That(task1, Is.Not.Null);
                Assert.That(task1, Is.TypeOf<TestTask>());
            }
        }

        [Test]
        public void Add_Specific_Task()
        {
            ITaskFactory subject;
            using (CreateSubject(out subject, tasks => tasks.Clear().Task<TestTask>()))
            {
                Assert.That(subject.Get<TestTask>(), Is.Not.Null);

                ITask[] tasks = subject.GetAll();
                Assert.That(tasks.Length, Is.EqualTo(1));
            }
        }

        [Test]
        public void Scan_Tasks_Add_Ignore()
        {
            ITaskFactory subject;
            using (CreateSubject(out subject, tasks => AddFromTestAssembly(tasks).Remove<TestTask>().Task<AnotherTask>()))
            {
                TaskNotFoundException ex = Assert.Throws<TaskNotFoundException>(() => subject.Get<TestTask>());
                Assert.That(ex.Message, Is.StringContaining("TestTask"));

                ITask anotherTask = subject.Get<AnotherTask>();
                Assert.That(anotherTask, Is.Not.Null);

                ITask[] tasks = subject.GetAll();
                Assert.That(tasks.Count(x => x.Equals(anotherTask)), Is.EqualTo(1));
            }
        }

        [Test]
        public void Scan_Tasks_Add_Ignore_Clear()
        {
            ITaskFactory subject;
            using (CreateSubject(out subject, tasks => AddFromTestAssembly(tasks).Remove<TestTask>().Task<AnotherTask>().Clear()))
            {
                Assert.That(subject.GetAll().Length, Is.EqualTo(0));
            }
        }

        [Test]
        public void Scan_Add_TaskWithSteps()
        {
            ITaskFactory subject;
            using (CreateSubject(out subject, tasks => AddFromTestAssembly(tasks)
                .Task<TaskWithStepsTask, TaskWithStepsWorkItem>(task => task
                    .Step<Step1>()
                    .Step<Step2>())))
            {
                ITask task = subject.Get<TaskWithStepsTask>();
                Assert.That(task, Is.Not.Null);
            }
        }

        [Test]
        public void GetAll_No_Duplicates()
        {
            ITaskFactory subject;
            using (CreateSubject(out subject, tasks => AddFromTestAssembly(tasks).Task<TestTask>()))
            {
                string[] tasks = subject.GetAll().Select(x => x.Name()).ToArray();

                bool duplicates = tasks
                    .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .Any(x => x.Count() > 1);

                Assert.That(duplicates, Is.False, 
                    String.Format("One or more tasks are duplicated: {0}.", 
                        String.Join(", ", tasks)));
            }
        }

        private static TasksConfiguration AddFromTestAssembly(TasksConfiguration tasks)
        {
            // skip this because it requires a bit more setup to work (e.g).
            return tasks.Clear().AddFromAssemblyOfThis<TaskFactoryTester>();
        }

        private static IDisposable CreateSubject(out ITaskFactory subject, Action<TasksConfiguration> tasks)
        {
            var configuration = new ApplicationConfiguration()
                .Logging(x => x.Use<VoidLogger>())
                .Database(x => x.DisableIntegrationDb());

            configuration.Tasks(tasks);

            var container = CastleWindsor.Initialize(configuration);

            subject = container.Resolve<ITaskFactory>();

            return container;
        }

        public class TestTask : Task, IEquatable<TestTask>
        {
            private readonly Guid _id;

            public TestTask()
            {
                _id = Guid.NewGuid();
            }

            public override string Description
            {
                get { return String.Format("TestTask-{0:D}", _id); }
            }

            public override string ToString()
            {
                return Description;
            }

            public override void StartTask(ITaskExecutionContext context)
            {
                throw new NotSupportedException();
            }

            public bool Equals(TestTask other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return _id.Equals(other._id);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((TestTask)obj);
            }

            public override int GetHashCode()
            {
                return _id.GetHashCode();
            }
        }

        public class AnotherTask : Task
        {
            public override void StartTask(ITaskExecutionContext context)
            {
            }

            public override string Description
            {
                get { return String.Empty; }
            }
        }

        public class TaskWithStepsTask : Task<TaskWithStepsWorkItem>
        {
            public TaskWithStepsTask(IEnumerable<IStep<TaskWithStepsWorkItem>> steps) : base(steps)
            {
            }

            public override string Description
            {
                get { return String.Empty; }
            }

            public override TaskWithStepsWorkItem Start(ITaskExecutionContext context)
            {
                return new TaskWithStepsWorkItem();
            }
        }

        public class Step1 : Step<TaskWithStepsWorkItem>
        {
            public override string Description
            {
                get { return String.Empty; }
            }

            public override void Execute(TaskWithStepsWorkItem workItem, ITaskExecutionContext context)
            {
            }
        }

        public class Step2 : Step<TaskWithStepsWorkItem>
        {
            public override string Description
            {
                get { return String.Empty; }
            }

            public override void Execute(TaskWithStepsWorkItem workItem, ITaskExecutionContext context)
            {
            }
        }

        public class TaskWithStepsWorkItem
        {
        }
    }

}