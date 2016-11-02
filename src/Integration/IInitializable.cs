namespace Vertica.Integration
{
    public interface IInitializable<in T>
    {
        void Initialized(T context);
    }
}