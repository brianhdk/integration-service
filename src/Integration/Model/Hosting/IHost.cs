namespace Vertica.Integration.Model.Hosting
{
    public interface IHost
    {
        bool CanHandle(HostArguments args);

        void Handle(HostArguments args);

        string Description { get; }
    }
}