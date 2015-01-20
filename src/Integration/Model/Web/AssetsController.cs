using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace Vertica.Integration.Model.Web
{
    public class AssetsController : ApiController
    {
        private static readonly string Root = Path.Combine(
            new FileInfo(typeof(AssetsController).Assembly.Location).DirectoryName ?? String.Empty,
            "Portal");

        [Route("assets/{*path}")]
        public HttpResponseMessage Get(string path)
        {
            return ServePortalFile(Request, Path.Combine("Assets", path));
        }

        internal static HttpResponseMessage ServePortalFile(HttpRequestMessage request, string path)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (String.IsNullOrWhiteSpace(path)) throw new ArgumentException(@"Value cannot be null or empty.", "path");

#if DEBUG
            const string codeFileDirectory = @"..\..\..\..\Integration\Portal\";

            if (Directory.Exists(Path.Combine(Root, codeFileDirectory)))
                path = String.Concat(codeFileDirectory, path);
#endif

            path = Path.Combine(Root, path);

            var file = new FileInfo(path);

            if (!file.Exists)
                return request.CreateErrorResponse(HttpStatusCode.NotFound, "Resource not found.");

            HttpResponseMessage response = request.CreateResponse(HttpStatusCode.OK);

            response.Content = new StreamContent(file.OpenRead());

            string contentType = MimeMapping.GetMimeMapping(file.Name);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            return response;
        }
    }
}