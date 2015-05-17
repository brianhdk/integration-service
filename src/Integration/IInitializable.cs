namespace Vertica.Integration
{
    internal interface IInitializable<in T>
    {
        void Initialize(T context);
    }
}