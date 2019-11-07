using System;
using Vertica.Integration.Infrastructure.Features;
using Vertica.Integration.Infrastructure.Threading.DistributedMutex.Db;

namespace Vertica.Integration.Infrastructure.Database.Migrations.Features
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    internal sealed class DbDistributedMutexFeatureAttribute : FeatureAttribute
    {
        public override void Disable(IFeatureToggler toggler)
        {
            toggler.Disable<DbDistributedMutex>();
        }

        public override void Enable(IFeatureToggler toggler)
        {
            toggler.Enable<DbDistributedMutex>();
        }
    }
}