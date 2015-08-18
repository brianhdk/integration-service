using System.Web.Http;
using Castle.MicroKernel;
using Owin;

namespace Vertica.Integration.WebApi.Infrastructure
{
	public interface IOwinConfiguration
	{
		IAppBuilder App { get; }
		HttpConfiguration Http { get; }
		IKernel Kernel { get; }
	}
}