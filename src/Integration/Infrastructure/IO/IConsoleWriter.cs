namespace Vertica.Integration.Infrastructure.IO
{
    public interface IConsoleWriter
    {
        void WriteLine(string format, params object[] args);
    }
}