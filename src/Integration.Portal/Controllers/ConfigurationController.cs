using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Database.Dapper;

namespace Vertica.Integration.Portal.Controllers
{
    public class ConfigurationController : ApiController
    {
        private readonly IDapperProvider _dapper;
	    private readonly IConfigurationProvider _configurationProvider;

        public ConfigurationController(IDapperProvider dapper, IConfigurationProvider configurationProvider)
        {
	        _dapper = dapper;
	        _configurationProvider = configurationProvider;
        }

	    public HttpResponseMessage Get()
	    {
		    var configs = _configurationProvider.GetAll();
            return Request.CreateResponse(HttpStatusCode.OK, configs);
        }

		public HttpResponseMessage Get(string clrType)
	    {
		    var config = _configurationProvider.Get(clrType);
            return Request.CreateResponse(HttpStatusCode.OK, config);
        }
    }
}
