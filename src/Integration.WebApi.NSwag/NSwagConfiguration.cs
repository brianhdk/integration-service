using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NJsonSchema;
using NSwag.AspNet.Owin;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.WebApi.Infrastructure;

namespace Vertica.Integration.WebApi.NSwag
{
    public class NSwagConfiguration
    {
        private readonly List<Assembly> _assemblies;
        private readonly SwaggerUiSettings _settings;
        private bool _disableUi;
        private Func<DisableIfCondition, bool> _disableIf;

        internal NSwagConfiguration(WebApiConfiguration webApi)
        {
            if (webApi == null) throw new ArgumentNullException(nameof(webApi));

            _assemblies = new List<Assembly>();

            _settings = new SwaggerUiSettings
            {
                DefaultUrlTemplate = "{controller}",
                UseJsonEditor = true,
                DefaultEnumHandling = EnumHandling.String
            };

            WebApi = webApi.HttpServer(httpServer => httpServer.Configure(Configure));
        }

        public WebApiConfiguration WebApi { get; }

        public NSwagConfiguration Title(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException(@"Value cannot be null or empty", nameof(title));
            
            return Settings(x => x.Title = title);
        }

        public NSwagConfiguration Description(string description)
        {
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException(@"Value cannot be null or empty", nameof(description));

            return Settings(x => x.Description = description);
        }

        public NSwagConfiguration Version(string version)
        {
            if (string.IsNullOrWhiteSpace(version)) throw new ArgumentException(@"Value cannot be null or empty", nameof(version));

            return Settings(x => x.Version = version);
        }

        /// <summary>
        /// Adds ApiControllers from the specified assembly.
        /// </summary>
        public NSwagConfiguration AddFromAssemblyOfThis<T>()
        {
            _assemblies.Add(typeof(T).Assembly);

            return this;
        }

        public NSwagConfiguration Settings(Action<SwaggerSettings> settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            settings(_settings);

            return this;
        }

        public NSwagConfiguration UiSettings(Action<SwaggerUiSettings> uiSettings)
        {
            if (uiSettings == null) throw new ArgumentNullException(nameof(uiSettings));

            uiSettings(_settings);

            return this;
        }

        public NSwagConfiguration DisableUi()
        { 
            _disableUi = true;

            return this;
        }

        public NSwagConfiguration DisableIf(Func<DisableIfCondition, bool> condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));

            _disableIf = condition;

            return this;
        }

        private void Configure(IOwinConfiguration owin)
	    {
		    if (owin == null) throw new ArgumentNullException(nameof(owin));

	        if (_assemblies.Count == 0)
	            return;

	        if (_disableIf != null && _disableIf(new DisableIfCondition(owin.Kernel)))
	        {
	            var console = owin.Kernel.Resolve<IConsoleWriter>();
	            console.WriteLine("[WebApi]: [NSwag]: Disabled");

	            return;
	        }

	        owin.App.UseSwagger(_assemblies.Distinct(), _settings);

	        if (!_disableUi)
	            owin.App.UseSwaggerUi(_assemblies.Distinct(), _settings);
        }
    }
}