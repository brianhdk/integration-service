using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Infrastructure.Remote.Ftp
{
    internal class FtpClient : IFtpClient
    {
        private readonly FtpClientConfiguration _configuration;
        private readonly Stack<Stack<string>> _paths;

        public FtpClient(FtpClientConfiguration configuration)
        {
            _configuration = configuration;
            _paths = new Stack<Stack<string>>();
        }

        public string NavigateTo(string path)
        {
            if (String.IsNullOrWhiteSpace(path)) throw new ArgumentException(@"Value cannot be null or empty.", "path");

            path = path.TrimStart('/');

            _paths.Push(new Stack<string>(new[] { path }));

            return CurrentPath;
        }

        public string NavigateBack()
        {
            if (_paths.Count > 0)
            {
                Stack<string> current = _paths.Peek();

                if (current.Count > 1)
                    current.Pop();
                else
                    _paths.Pop();                
            }
            
            return CurrentPath;
        }

        public string NavigateDown(string name)
        {
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");

            name = name.TrimStart('/');

            if (_paths.Count == 0)
                return NavigateTo(name);

            _paths.Peek().Push(name);

            return CurrentPath;
        }

        public string CurrentPath
        {
            get { return String.Concat("/", _paths.Count > 0 ? String.Join("/", _paths.Peek().Reverse().ToArray()) : String.Empty); }
        }

        public string[] ListDirectory(Func<string, bool> predicate = null)
        {
            return WithExceptionHandling(() =>
            {
                var result = new List<string>();

                FtpWebRequest request = CreateRequest();
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream ?? Stream.Null))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine() ?? String.Empty;

                        if (predicate == null || predicate(line))
                            result.Add(line);
                    }
                }

                return result.ToArray();
            });
        }

        public string CreateDirectory(string name)
        {
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");

            using (Enter(name))
            {
                return WithExceptionHandling(() =>
                {
                    FtpWebRequest request = CreateRequest();
                    request.Method = WebRequestMethods.Ftp.MakeDirectory;

                    using (request.GetResponse())
                    {
                        // request is fired creating new directory.
                        return CurrentPath;
                    }
                });
            }
        }

        public string CreateDirectoryAndEnterIt(string name)
        {
            return NavigateTo(CreateDirectory(name));
        }

        public string Upload(string name, Stream data, bool binary = true)
        {
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");
            if (data == null) throw new ArgumentNullException("data");

            using (Enter(name))
            {
                return WithExceptionHandling(() =>
                {
                    FtpWebRequest request = CreateRequest();
                    request.UseBinary = binary;
                    request.Method = WebRequestMethods.Ftp.UploadFile;

                    using (Stream requestStream = request.GetRequestStream() ?? Stream.Null)
                    {
                        data.CopyTo(requestStream);

                        return CurrentPath;
                    }
                });
            }
        }

        public string Download(string name, Action<Stream> data)
        {
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");
            if (data == null) throw new ArgumentNullException("data");

            using (Enter(name))
            {
                return WithExceptionHandling(() =>
                {
                    FtpWebRequest request = CreateRequest();
                    request.Method = WebRequestMethods.Ftp.DownloadFile;

                    using (FtpWebResponse response = (FtpWebResponse) request.GetResponse())
                    using (Stream responseStream = response.GetResponseStream() ?? Stream.Null)
                    {
                        data(responseStream);

                        return CurrentPath;
                    }
                });
            }
        }

        public string DeleteFile(string name)
        {
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");

            using (Enter(name))
            {
                return WithExceptionHandling(() =>
                {
                    FtpWebRequest request = CreateRequest();
                    request.Method = WebRequestMethods.Ftp.DeleteFile;

                    using (request.GetResponse())
                    {
                        // request is fired deleting file.
                        return CurrentPath;
                    }
                });
            }
        }

        public string DeleteDirectory(string name)
        {
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");

            using (Enter(name))
            {
                return WithExceptionHandling(() =>
                {
                    FtpWebRequest request = CreateRequest();
                    request.Method = WebRequestMethods.Ftp.RemoveDirectory;

                    using (request.GetResponse())
                    {
                        // request is fired deleting file.
                        return CurrentPath;
                    }                    
                });
            }
        }

        private T WithExceptionHandling<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                MethodBase method = new StackFrame(1).GetMethod();
                string caller = String.Format("{0}.{1}", method.DeclaringType != null ? method.DeclaringType.Name : "n/a", method.Name);

                throw new FtpClientException(caller, CurrentPath, ex);
            }
        }

        private IDisposable Enter(string name)
        {
            NavigateDown(name);

            return new DisposableAction(() => NavigateBack());
        }

        private FtpWebRequest CreateRequest()
        {
            FtpWebRequest request = _configuration.CreateRequest(CurrentPath);

            return request;
        }
    }
}