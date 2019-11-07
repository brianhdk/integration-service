using System;
using System.Net;

namespace Vertica.Integration.Infrastructure.Remote.Ftp
{
    public class FtpClientConfiguration
    {
        private readonly Func<string, FtpWebRequest> _request;

        private NetworkCredential _credentials;

        internal FtpClientConfiguration(string uri)
        {
            _request = path => (FtpWebRequest) WebRequest.Create(BuildPath(uri, path));

            AssertPath();
        }

        internal FtpClientConfiguration(Uri uri)
        {
            _request = path =>
            {
                var builder = new UriBuilder(uri);
                builder.Path = BuildPath(builder.Path, path);

                return (FtpWebRequest) WebRequest.Create(builder.Uri);
            };

            AssertPath();
        }

        private void AssertPath()
        {
            _request(string.Empty);
        }

        public FtpClientConfiguration Credentials(string username, string password)
        {
            _credentials = new NetworkCredential(username, password);

            return this;
        }

        internal FtpWebRequest CreateRequest(string path)
        {
            FtpWebRequest request = _request(path);

			if (_credentials != null)
				request.Credentials = _credentials;

            return request;
        }

        private static string BuildPath(string basePath, string appendPath)
        {
            return string.Concat(basePath, appendPath);
        }
    }
}