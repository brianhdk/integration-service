using System;
using Vertica.Integration.Infrastructure.Features;

namespace Vertica.Integration.Infrastructure.Database.Migrations.Features
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal abstract class FeatureAttribute : Attribute
    {
        public abstract void Disable(IFeatureToggler toggler);
        public abstract void Enable(IFeatureToggler toggler);
    }
}