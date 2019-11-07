using System.Web.Http;
using Castle.MicroKernel;
using Microsoft.Owin.BuilderProperties;
using Owin;

namespace Vertica.Integration.WebApi.Infrastructure
{
	public interface IOwinConfiguration
	{
		IAppBuilder App { get; }
        AppProperties Properties { get; }
        HttpConfiguration Http { get; }
		IKernel Kernel { get; }
	}
}