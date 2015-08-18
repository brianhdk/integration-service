using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace Integration.WebApi.SignalR.SmokeTester2
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

            var file = new FileInfo(Path.Combine(PortalConfiguration.Folder, relativePathToFile));

            if (!file.Exists)
                return request.CreateErrorResponse(HttpStatusCode.NotFound, "Resource not found.");

            HttpResponseMessage response = request.CreateResponse(HttpStatusCode.OK);

            response.Content = new StreamContent(file.OpenRead());

            string contentType = MimeMapping.GetMimeMapping(file.Name);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            SetCacheControl(response);

            return response;
        }

        private static void SetCacheControl(HttpResponseMessage response)
        {
            response.Headers.CacheControl = new CacheControlHeaderValue
            {
				NoCache = true,
				NoStore = true,
                MaxAge = TimeSpan.Zero,
                MustRevalidate = true,
                Private = true
            };

			response.Headers.Pragma.Add(new NameValueHeaderValue("no-cache"));
	        response.Content.Headers.Expires = new DateTime(1990, 01, 01);
        }
    }
}