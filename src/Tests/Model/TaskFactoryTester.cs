using System;
using System.Linq;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;

namespace Vertica.Integration.Tests.Model
{
    [TestFixture(Category = "Integration")]
    public class TaskFactoryTester
    {
        [Test]
        public void GetByName_Verify_SingletonBehaviour()
        {
            ITaskFactory subject;
            using (CreateSubject(out subject))
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
            using (CreateSubject(out subject))
            {
                ITask task1 = subject.GetByName("testtask");

                Assert.That(task1, Is.Not.Null);
                Assert.That(task1, Is.TypeOf<TestTask>());
            }
        }

        [Test]
        public void GetAll_No_Duplicates()
        {
            ITaskFactory subject;
            using (CreateSubject(out subject))
            {
                string[] tasks = subject.GetAll().Select(x => x.Name()).ToArray();

                Assert.That(tasks.Length, Is.GreaterThan(1));

                bool duplicates = tasks
                    .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .Any(x => x.Count() > 1);

                Assert.That(duplicates, Is.False, 
                    String.Format("One or more tasks are duplicated: {0}.", 
                        String.Join(", ", tasks)));
            }
        }

        private static IDisposable CreateSubject(out ITaskFactory subject)
        {
            var configuration = new ApplicationConfiguration();
            configuration.Tasks(x => x.Skip<MigrateTask>()); // skip this because it requires a bit more setup to work (e.g).
            configuration.Tasks(x => x.AddTasksFromAssemblyOfThis<TaskFactoryTester>());

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

            public override void StartTask(ILog log, params string[] arguments)
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
    }
}