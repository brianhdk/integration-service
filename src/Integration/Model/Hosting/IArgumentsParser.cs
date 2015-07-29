namespace Vertica.Integration.Model.Hosting
{
    public interface IArgumentsParser
    {
        HostArguments Parse(string[] arguments);
    }
}