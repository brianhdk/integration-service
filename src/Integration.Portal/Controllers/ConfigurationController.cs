using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Configuration;

namespace Vertica.Integration.Portal.Controllers
{
    public class ConfigurationController : ApiController
    {
	    private readonly IConfigurationService _service;
	    private readonly IConfigurationRepository _repository;

	    public ConfigurationController(IConfigurationService service, IConfigurationRepository repository)
        {
	        _service = service;
	        _repository = repository;
        }

	    public HttpResponseMessage Get()
	    {
		    Configuration[] model = _repository.GetAll();

            return Request.CreateResponse(HttpStatusCode.OK, model);
        }

		public HttpResponseMessage Get(string id)
	    {
		    Configuration model = _repository.Get(id);

            if (model == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            return Request.CreateResponse(HttpStatusCode.OK, model);
        }

        public HttpResponseMessage Put(Configuration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            // TODO: Validate JSON before submitting

	        _service.Backup(configuration.Id);

	        configuration.UpdatedBy = "Portal";
            configuration = _repository.Save(configuration);

            return Request.CreateResponse(HttpStatusCode.OK, configuration);
        }

        public HttpResponseMessage Delete(string id)
        {
            Configuration configuration = _repository.Get(id);

            if (configuration == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            Put(configuration);
            _repository.Delete(id);

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}