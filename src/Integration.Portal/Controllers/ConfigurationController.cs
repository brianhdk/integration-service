using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Configuration;

namespace Vertica.Integration.Portal.Controllers
{
    public class ConfigurationController : ApiController
    {
	    private readonly IConfigurationProvider _configurationProvider;

        public ConfigurationController(IConfigurationProvider configurationProvider)
        {
	        _configurationProvider = configurationProvider;
        }

	    public HttpResponseMessage Get()
	    {
		    Configuration[] model = _configurationProvider.GetAll();

            return Request.CreateResponse(HttpStatusCode.OK, model);
        }

		public HttpResponseMessage Get(string clrType)
	    {
		    Configuration model = _configurationProvider.Get(clrType);

            if (model == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            return Request.CreateResponse(HttpStatusCode.OK, model);
        }

        public HttpResponseMessage Put(Configuration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            // TODO: Validate JSON before submitting

            configuration.UpdatedBy = "Portal";
            configuration = _configurationProvider.Save(configuration, createArchiveBackup: true);

            return Request.CreateResponse(HttpStatusCode.OK, configuration);
        }
    }
}