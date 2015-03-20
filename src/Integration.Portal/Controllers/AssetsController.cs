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
            return ServeFile(Request, path);
        }

        internal static HttpResponseMessage ServeFile(HttpRequestMessage request, string relativePathToFile)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (String.IsNullOrWhiteSpace(relativePathToFile)) throw new ArgumentException(@"Value cannot be null or empty.", "relativePathToFile");

            relativePathToFile = Path.Combine(PortalConfiguration.AssetsFolderName, relativePathToFile);

#if DEBUG
            const string developmentFolder = @"..\..\..\Integration.Portal";

            if (Directory.Exists(Path.Combine(BinFolder, developmentFolder)))
                relativePathToFile = String.Concat(developmentFolder, "\\", relativePathToFile);
#endif

            relativePathToFile = Path.Combine(PortalConfiguration.BinFolder, relativePathToFile);

            var file = new FileInfo(relativePathToFile);

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