namespace Vertica.Integration.Infrastructure.Features
{
    public interface IFeatureToggler
    {
        void Disable<TFeature>();
        void Enable<TFeature>();

        bool IsDisabled<TFeature>();
    }
}