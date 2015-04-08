using System;
using System.IO;

namespace Vertica.Integration.Infrastructure.Remote.Ftp
{
    public interface IFtpClient
    {
        string CurrentPath { get; }

        string NavigateTo(string path);
        string NavigateBack();
        string NavigateDown(string name);

        string[] ListDirectory(Func<string, bool> predicate = null);
        string CreateDirectory(string name);
        string CreateDirectoryAndEnterIt(string name);

        string Upload(string name, Stream data, bool binary = true);
        string Download(string name, Action<Stream> data);

        string DeleteFile(string name);
        string DeleteDirectory(string name);
    }
}