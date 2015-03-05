using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;

namespace Vertica.Integration.Model.Web
{
    public class WebApiConfiguration
    {
        private readonly List<Assembly> _scan;
        private readonly List<Type> _add;
        private readonly List<Type> _remove; 

        public WebApiConfiguration()
        {
            _scan = new List<Assembly>();
            _add = new List<Type>();
            _remove = new List<Type>();

            // scan own assembly
            Scan(GetType().Assembly);
        }

        public WebApiConfiguration Scan(Assembly assemblyToScan)
        {
            if (assemblyToScan == null) throw new ArgumentNullException("assemblyToScan");

            _scan.Add(assemblyToScan);

            return this;
        }

        public WebApiConfiguration Add<T>()
            where T : ApiController
        {
            _add.Add(typeof (T));

            return this;
        }

        public WebApiConfiguration Remove<T>()
            where T : ApiController
        {
            _remove.Add(typeof (T));

            return this;
        }

        internal Assembly[] ScanAssemblies
        {
            get { return _scan.Distinct().ToArray(); }
        }

        internal Type[] AddControllers
        {
            get { return _add.Except(_remove).Distinct().ToArray(); }
        }

        internal Type[] RemoveControllers
        {
            get { return _remove.Distinct().ToArray(); }
        }
    }
}