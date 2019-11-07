using System;
using Vertica.Integration.Infrastructure.Features;
using Vertica.Integration.Infrastructure.Logging.Loggers;

namespace Vertica.Integration.Infrastructure.Database.Migrations.Features
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    internal sealed class DbLoggerFeatureAttribute : FeatureAttribute
    {
        public override void Disable(IFeatureToggler toggler)
        {
            toggler.Disable<DbLogger>();
        }

        public override void Enable(IFeatureToggler toggler)
        {
            toggler.Enable<DbLogger>();
        }
    }
}