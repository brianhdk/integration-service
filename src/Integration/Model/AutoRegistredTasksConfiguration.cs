using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Vertica.Integration.Model
{
    public class AutoRegistredTasksConfiguration
    {
        private readonly List<Assembly> _scan;
        private readonly List<Type> _skip; 

        public AutoRegistredTasksConfiguration()
        {
            _scan = new List<Assembly>();
            _skip = new List<Type>();

            // scan own assembly
            Scan(GetType().Assembly);
        }

        public AutoRegistredTasksConfiguration Scan(Assembly assemblyToScan)
        {
            if (assemblyToScan == null) throw new ArgumentNullException("assemblyToScan");

            _scan.Add(assemblyToScan);

            return this;
        }

        public AutoRegistredTasksConfiguration Skip<T>()
            where T : Task
        {
            _skip.Add(typeof (T));

            return this;
        }

        internal Assembly[] ScanAssemblies
        {
            get { return _scan.Distinct().ToArray(); }
        }

        internal Type[] Skipped
        {
            get { return _skip.Distinct().ToArray(); }
        }
    }
}