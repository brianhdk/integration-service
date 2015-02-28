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
		    Configuration[] configurations = _configurationProvider.GetAll();

            return Request.CreateResponse(HttpStatusCode.OK, configurations);
        }

		public HttpResponseMessage Get(string clrType)
	    {
		    Configuration configuration = _configurationProvider.Get(clrType);

            return Request.CreateResponse(HttpStatusCode.OK, configuration);
        }

        public HttpResponseMessage Put(Configuration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            // TODO: Validate JSON before submitting

            configuration.UpdatedBy = "Portal";
            
            configuration = _configurationProvider.Save(configuration);

            return Request.CreateResponse(HttpStatusCode.OK, configuration);
        }
    }
}
