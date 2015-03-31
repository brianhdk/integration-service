using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Model
{
    public class TasksConfiguration
    {
        private readonly List<Assembly> _scan;
        private readonly List<Type> _skip;

        internal TasksConfiguration()
        {
            var configurationFile = new FileInfo("Tasks.config");

            if (configurationFile.Exists)
                ConfigurationFileName = configurationFile.Name;

            _scan = new List<Assembly>();
            _skip = new List<Type>();

            // scan own assembly
            ScanFromAssemblyOfThis<TasksConfiguration>();
        }

        public string ConfigurationFileName { get; set; }

        public TasksConfiguration ScanFromAssemblyOfThis<T>()
        {
            _scan.Add(typeof(T).Assembly);

            return this;
        }

        public TasksConfiguration Skip<T>()
            where T : Task
        {
            _skip.Add(typeof (T));

            return this;
        }

        public TasksConfiguration Change(Action<TasksConfiguration> change)
        {
            if (change != null)
                change(this);

            return this;
        }

        internal void Install(IWindsorContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");

            container.Install(new AutoRegisterTasksInstaller(ScanAssemblies, SkipTypes));
            container.Install(new TaskFactoryInstaller());
        }

        private Type[] SkipTypes
        {
            get { return _skip.Distinct().ToArray(); }
        }

        private Assembly[] ScanAssemblies
        {
            get { return _scan.Distinct().ToArray(); }
        }
    }
}