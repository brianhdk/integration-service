using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Vertica.Integration.Model.Web
{
    public class WebApiConfiguration
    {
        private readonly List<Assembly> _assemblies;
        private readonly List<Type> _controllers;

        public WebApiConfiguration()
        {
            _assemblies = new List<Assembly>();
            _controllers = new List<Type>();
        }

        public WebApiConfiguration ScanAssembly(Assembly assemblyToScan)
        {
            if (assemblyToScan == null) throw new ArgumentNullException("assemblyToScan");

            _assemblies.Add(assemblyToScan);

            return this;
        }

        public WebApiConfiguration AddApiController(Type apiController)
        {
            if (apiController == null) throw new ArgumentNullException("apiController");

            _controllers.Add(apiController);

            return this;
        }

        public Assembly[] Assemblies
        {
            get { return _assemblies.Distinct().ToArray(); }
        }

        public Type[] Controllers
        {
            get { return _controllers.Distinct().ToArray(); }
        }
    }
}