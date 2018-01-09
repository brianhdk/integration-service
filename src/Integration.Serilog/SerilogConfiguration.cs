using System;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Serilog;
using Serilog.Core;

namespace Vertica.Integration.Serilog
{
    public class SerilogConfiguration : IInitializable<IWindsorContainer>
    {
        private Action<IKernel, LoggerConfiguration> _configuration;
        private bool _skipSetGlobalStaticLogger;

        internal SerilogConfiguration(ApplicationConfiguration application)
	    {
		    if (application == null) throw new ArgumentNullException(nameof(application));

            Application = application;
	    }

		public ApplicationConfiguration Application { get; }

        public SerilogConfiguration Configure(Action<IKernel, LoggerConfiguration> configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            _configuration = configuration;

            return this;
        }

        /// <summary>
        /// This will not set the globally-shared <code>Log.Logger</code> property with the ILogger instance.
        /// </summary>
        /// <returns></returns>
        public SerilogConfiguration SkipSetGlobalStaticLogger()
        {
            _skipSetGlobalStaticLogger = true;

            return this;
        }

        void IInitializable<IWindsorContainer>.Initialized(IWindsorContainer container)
		{
		    container.Register(Component
                .For<ILogger>()
		        .UsingFactoryMethod(LoggerConfig)
		        .OnDestroy(x => Log.CloseAndFlush()));
        }

        private ILogger LoggerConfig(IKernel kernel)
        {
            var configuration = new LoggerConfiguration();
            _configuration?.Invoke(kernel, configuration);

            Logger logger = configuration.CreateLogger();
            
            if (!_skipSetGlobalStaticLogger)
                Log.Logger = logger;

            return logger;
        }
    }
}