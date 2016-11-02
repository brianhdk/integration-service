using System;

namespace Vertica.Integration.IIS
{
    public class IISConfiguration
    {
        internal IISConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

            Application = application
                .Services(services => services
                    .Conventions(conventions => conventions
                        .AddFromAssemblyOfThis<IISConfiguration>()));
        }

        public IISConfiguration Change(Action<IISConfiguration> change)
        {
            change?.Invoke(this);

            return this;
        }

        public ApplicationConfiguration Application { get; }
    }
}