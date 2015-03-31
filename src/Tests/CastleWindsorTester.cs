using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using NUnit.Framework;
using Vertica.Integration.Model;

namespace Vertica.Integration.Tests
{
    [TestFixture(Category = "Integration")]
    public class CastleWindsorTester
    {
        [Test]
        public void Initialize_ConfigurationFileNotFound_Throws()
        {
            var configuration = new ApplicationConfiguration();
            configuration.Tasks(x => x.ConfigurationFileName = String.Format("NotFound-{0}.config", Guid.NewGuid()));

            Assert.Throws<ConfigurationProcessingException>(() => CastleWindsor.Initialize(configuration));
        }

        [Test]
        public void Initialize_ResetConfigurationFile_DoesNotThrow()
        {
            var configuration = new ApplicationConfiguration()
                .Tasks(tasks => tasks.ConfigurationFileName = null);

            Assert.DoesNotThrow(() => CastleWindsor.Initialize(configuration));
        }

        [Test]
        public void Initialize_CustomConfigurationFile_DoesNotThrow()
        {
            var file = new FileInfo(String.Format("{0:N}.config", Guid.NewGuid()));

            File.WriteAllText(file.Name, @"
<?xml version=""1.0""?>
<configuration>
	<components>
        <component id=""TestWithStepsTask""
		           service=""Vertica.Integration.Model.ITask, Vertica.Integration""
		           type=""Vertica.Integration.Tests.CastleWindsorTester+TestWithStepsTask, Vertica.Integration.Tests"">
			<parameters>
				<steps>
					<list>
						<item>${TestStep}</item>
					</list>
				</steps>
			</parameters>
		</component>
        <component id=""TestStep""
		           service=""Vertica.Integration.Model.IStep, Vertica.Integration""
		           type=""Vertica.Integration.Tests.CastleWindsorTester+TestStep, Vertica.Integration.Tests"" />
	</components>
</configuration>".Trim());

            try
            {
                var configuration = new ApplicationConfiguration()
                    .Tasks(tasks => tasks.ConfigurationFileName = file.Name);

                IWindsorContainer container = CastleWindsor.Initialize(configuration);
                ITaskFactory factory = container.Resolve<ITaskFactory>();

                Assert.DoesNotThrow(() => factory.GetByName("TestWithStepsTask"));
            }
            finally
            {
                if (File.Exists(file.Name))
                    File.Delete(file.Name);
            }
        }

        public class TestWithStepsTask : Task<TestWithStepsWorkItem>
        {
            public TestWithStepsTask(IEnumerable<IStep<TestWithStepsWorkItem>> steps)
                : base(steps)
            {
                Assert.That(steps.Count(), Is.EqualTo(1));
            }

            public override string Description
            {
                get { return "TBD"; }
            }

            public override TestWithStepsWorkItem Start(Log log, params string[] arguments)
            {
                throw new NotImplementedException();
            }
        }

        public class TestStep : Step<TestWithStepsWorkItem>
        {
            public override string Description
            {
                get { return "TBD"; }
            }

            public override void Execute(TestWithStepsWorkItem workItem, Log log)
            {
                throw new NotImplementedException();
            }
        }

        public class TestWithStepsWorkItem
        {
        }
    }
}