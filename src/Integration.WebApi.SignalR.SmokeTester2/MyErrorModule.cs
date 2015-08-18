using System;
using Microsoft.AspNet.SignalR.Hubs;

namespace Integration.WebApi.SignalR.SmokeTester2
{
	// http://stackoverflow.com/questions/21796087/signalr-owin-and-exception-handling
	public class MyErrorModule : HubPipelineModule
	{
		public MyErrorModule()
		{
			string s = "";
		}

		protected override void OnIncomingError(ExceptionContext exceptionContext, IHubIncomingInvokerContext invokerContext)
		{
			base.OnIncomingError(exceptionContext, invokerContext);

			MethodDescriptor method = invokerContext.MethodDescriptor;

			Console.WriteLine("{0}.{1}({2}) threw the following uncaught exception: {3}",
				method.Hub.Name,
				method.Name,
				String.Join(", ", invokerContext.Args),
				exceptionContext.Error);
		}
	}
}