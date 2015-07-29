namespace Vertica.Integration
{
    public interface IInitializable<in T>
    {
        void Initialize(T context);
    }
}