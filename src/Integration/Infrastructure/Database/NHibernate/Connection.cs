using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentNHibernate.Cfg;
using NHibernate;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration.Infrastructure.Database.NHibernate
{
	public abstract class Connection
	{
		private readonly string _connectionStringName;

	    protected Connection(string connectionStringName)
		{
			_connectionStringName = connectionStringName;
		}

	    protected internal virtual void OnSessionFactoryCreated(ISessionFactory sessionFactory)
		{
		}

	    internal protected virtual void SetupConfiguration(global::NHibernate.Cfg.Configuration config)
	    {
	    }

	    internal void SetupMappings(MappingConfiguration configuration)
		{
			// Auto Mapping strategy: http://wiki.fluentnhibernate.org/Auto_mapping
			//configuration.AutoMappings.Add(AutoMap.Assemblies(_assembliesContainingMappings.ToArray()));

			// Fluent Mapping strategy: http://wiki.fluentnhibernate.org/Fluent_mapping
			foreach (Assembly assembly in MappingAssemblies.EmptyIfNull().Distinct())
				configuration.FluentMappings.AddFromAssembly(assembly);

		    foreach (Type type in MappingTypes.EmptyIfNull().Distinct())
		        configuration.FluentMappings.Add(type);
		}

	    protected abstract IEnumerable<Assembly> MappingAssemblies { get; }
        protected abstract IEnumerable<Type> MappingTypes { get; }

		internal string ConnectionStringName
		{
			get { return _connectionStringName; }
		}

		internal string SessionFactoryName
		{
			get { return String.Format("NHibernate.SessionFactory.{0}", ConnectionStringName); }
		}

		internal string CurrentSessionName
		{
			get { return String.Format("NHibernate.CurrentSession.{0}", ConnectionStringName); }
		}

		internal string SelectorName
		{
			get { return String.Format("NHibernate.Selector.{0}", ConnectionStringName); }
		}

		internal string FactoryName
		{
			get { return String.Format("NHibernate.Factory.{0}", ConnectionStringName); }
		}
	}
}
