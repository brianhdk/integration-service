using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Archiving;

namespace Vertica.Integration.Portal.Controllers
{
    public class ArchivesController : ApiController
    {
        private readonly IArchiveService _archive;

        public ArchivesController(IArchiveService archive)
        {
            _archive = archive;
        }

        public HttpResponseMessage Get()
        {
            Archive[] archives = _archive.GetAll();

            return Request.CreateResponse(HttpStatusCode.OK, archives);
        }

        public HttpResponseMessage Get(string id)
        {
            byte[] archive = _archive.Get(id);

            if (archive == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);

            Stream stream = new MemoryStream(archive);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = string.Format("Archive-{0}.zip", id)
            };

            return response;
        }
    }
}
