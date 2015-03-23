using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace Vertica.Integration.Portal.Controllers
{
    public class AssetsController : ApiController
    {
        [Route("assets/{*path}")]
        public HttpResponseMessage Get(string path)
        {
            return ServeFile(Request, Path.Combine("Assets", path));
        }

        internal static HttpResponseMessage ServeFile(HttpRequestMessage request, string relativePathToFile)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (String.IsNullOrWhiteSpace(relativePathToFile)) throw new ArgumentException(@"Value cannot be null or empty.", "relativePathToFile");

            var file = new FileInfo(Path.Combine(PortalConfiguration.BaseFolder, relativePathToFile));

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