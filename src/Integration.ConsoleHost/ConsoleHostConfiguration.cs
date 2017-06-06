using System;
using Castle.Windsor;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.ConsoleHost
{
    internal class ConsoleHostConfiguration : IInitializable<IWindsorContainer>
    {
        internal ConsoleHostConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

            Application = application
                //.Services(services => services
                //    .Interceptors(interceptors => interceptors
                //        .InterceptService<IExecute, WindowsServiceExecuteInterceptor>()))
            ;
        }

        public ApplicationConfiguration Application { get; }

        void IInitializable<IWindsorContainer>.Initialized(IWindsorContainer container)
        {
            // SKAL WRAPPE EXECUTE METODEN - da denne er den eneste metode som modtager ARGS!
                // - og dermed kan finde ud af om den skal installere/afinstallere/køre

            // Environment.GetCommandLineArgs()


            // TODO:
            /*
             * Figure out a way to embed everything in a Windows Service context
             * Obsolete API's in the "native" integration class library project that has something to do with Windows Services
             * 
             * IWindowsServiceHandler ?
             * 
             * Start as early as possible // let windows service know that we're running
             * 
             * Right now, windows service kicks in at:
             *  
            */
        }
    }
}