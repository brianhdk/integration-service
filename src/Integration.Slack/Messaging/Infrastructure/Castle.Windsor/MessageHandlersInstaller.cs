using Castle.MicroKernel.Registration;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Slack.Messaging.Handlers;

namespace Vertica.Integration.Slack.Messaging.Infrastructure.Castle.Windsor
{
    internal class MessageHandlersInstaller : ScanAddRemoveInstaller<IHandleMessages>
    {
        public MessageHandlersInstaller()
            : base(ServiceDescriptor, Configure)
        {
            Add<SlackChannelApi>();
        }

        private static void Configure(ComponentRegistration registration)
        {
            registration
                .LifestyleSingleton();
        }

        private static BasedOnDescriptor ServiceDescriptor(ServiceDescriptor arg)
        {
            return arg
                .FromInterface(typeof(IHandleMessages<>))
                .WithServiceAllInterfaces();
        }
    }
}