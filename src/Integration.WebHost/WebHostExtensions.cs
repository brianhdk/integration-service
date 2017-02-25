using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Vertica.Integration.Infrastructure;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Vertica.Integration.WebHost
{
    public static class WebHostExtensions
    {
        public const string ContextKey = "integrationService.ApplicationContext";

        public static IAppBuilder UseIntegrationService(this IAppBuilder app, Action<ApplicationConfiguration> application = null)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            OwinContext owinContext = new OwinContext(app.Properties);

            CancellationToken cancellationToken = GetCancellationToken(owinContext);

            IApplicationContext applicationContext = ApplicationContext.Create(applicationCopy =>
            {
                application?.Invoke(applicationCopy);

                applicationCopy.Services(services => services
                    .Advanced(advanced => advanced
                        .Register<IWaitForShutdownRequest>(kernel => 
                            new WebHostShutdownRequest(cancellationToken))));
            });

            cancellationToken.Register(() => applicationContext.Dispose());

            // Make the application context instance available on each request
            app.Use(new Func<AppFunc, AppFunc>(next => (async env =>
            {
                env.Add(ContextKey, applicationContext);
                await next.Invoke(env);
            })));

            owinContext.Set(ContextKey, applicationContext);

            return app;
        }

        public static IAppBuilder RunIntegrationService(this IAppBuilder app, params string[] args)
        {
            OwinContext owinContext = new OwinContext(app.Properties);

            IApplicationContext applicationContext = owinContext.GetIntegrationService();

            CancellationToken cancellationToken = GetCancellationToken(owinContext);

            Task.Run(() => applicationContext.Execute(args), cancellationToken);
            
            return app;
        }

        public static IApplicationContext GetIntegrationService(this IOwinContext owin)
        {
            if (owin == null) throw new ArgumentNullException(nameof(owin));

            IApplicationContext context = owin.Get<IApplicationContext>(ContextKey);

            if (context == null)
                throw new InvalidOperationException($"ApplicationContext has not been set. Did you remember to run the '{nameof(UseIntegrationService)}'-method part of bootstrapping Owin.");

            return context;
        }

        private static CancellationToken GetCancellationToken(IOwinContext owin)
        {
            CancellationToken cancellationToken = owin.Get<CancellationToken>("host.OnAppDisposing");

            if (cancellationToken.Equals(CancellationToken.None))
                cancellationToken = owin.Get<CancellationToken>("server.OnDispose");

            if (cancellationToken.Equals(CancellationToken.None))
                throw new InvalidOperationException("Current OWIN environment does not contain an instance of the `CancellationToken` class neither under `host.OnAppDisposing`, nor `server.OnDispose` key.");

            return cancellationToken;
        }
    }
}