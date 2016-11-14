using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Slack.Bot.Commands.Infrastructure.Castle.Windsor
{
    internal class SlackBotCommandsInstaller : ScanAddRemoveInstaller<ISlackBotCommand>
    {
        public SlackBotCommandsInstaller()
        {
            Add<DumpArchiveCommand>();
            Add<PingCommand>();
            Add<RunTaskCommand>();
        }

        protected override void Install(IWindsorContainer container, IConfigurationStore store)
        {
            base.Install(container, store);

            container.Register(
                Component
                    .For<ISlackBotCommand[]>()
                    .UsingFactoryMethod(kernel => kernel.ResolveAll<ISlackBotCommand>())
                    .LifestyleSingleton());
        }
    }
}