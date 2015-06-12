# Integration Service
General purpose platform for running Tasks and Migrations expose (internally) HTTP Services and much much more!

## Table of Contents
 - [How to Get Started](#how-to-get-started)
 - [Basics of Tasks](#basics-of-tasks)
 - [Bootstrapping Tasks](#bootstrapping-tasks)
 - [Built-in Tasks](#built-in-tasks) 
 - [Task Execution Flow](#task-execution-flow)  
 - [Scheduling Tasks](#scheduling-tasks)   
 - [Basics of WebApi](#basics-of-webapi)
 - [Logging and Exceptions](#logging-and-exceptions) 
 - [Command Line Reference](#command-line-reference)
 - [Migrations](#migrations)
 - [Built-in Services](#built-in-services)
 - [Configurations](#configurations)
 - [Archives](#archives)
 - [CSV](#csv)
 - [FTP](#ftp)
 - [Setting up Portal](#setting-up-portal)
 - [Integrating Elmah](#integrating-elmah)
 - [Integrating Azure - BlobStorage](#integrating-azure---blobstorage)
 - [How to Disable IntegrationDb](#how-to-disable-integrationdb)
 - [How to Change Logger](#how-to-change-logger) 
 - [How to Register Custom dependencies/services](#how-to-register-custom-dependenciesservices)
 - [How to Setup connection to custom database](#how-to-setup-connection-to-custom-database) 

## How to Get Started

1. Choosing a Host for Integration Service
	Typically Integration Service is hosted through a simple .NET Console Application (.exe). Add a new "Console Application" project to your existing (or new solution). 

  **NOTE:** Later you'll add a Class Library project where all your actual implementation code will be placed.

2. Install Integration Service via NuGet
  ```
  Install-Package Vertica.Integration.Host
  ```
  The package above will add all necessary files to get you up and running soon.
3. Modify "Program.cs" as mentioned in the "Readme.txt"
  ```c#
  namespace ConsoleApplication16
  {
      class Program
      {
          static void Main(string[] args)
          {
              IntegrationStartup.Run(args);
          }
      }
  }
 
  ```
4. Open file app.config, and fill-out the [Placeholder]'s with actual values:
 
  ### Database configuration
  ```xml
  <connectionStrings>
      <add name="IntegrationDb" connectionString="Integrated Security=SSPI;Data Source=[NAME-OF-SQL-SERVER];Database=[NAME-OF-INTEGRATION-DATABASE]" />
  </connectionStrings>  
  ```

  Azure example:
  ```xml
  <add name="IntegrationDb" connectionString="Server=tcp:xxxx.database.windows.net,1433;Database=IntegrationDb;User ID=xxxx@xxxx;Password=xxxx;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" />
  ```  

  *By default Integration Service requires a database - but this can be disabled. See section [How to Disable IntegrationDb](#how-to-disable-integrationdb) to read more about the option of running Integration Service without a database.*
  
  ### SMTP
  ```xml
  <mailSettings>
      <smtp from="[EMAIL-ADDRESS]">
          <network host="[SMTP-HOST]" />
      </smtp>
  </mailSettings>
  ```
  
  Mandrill example:
  ```xml
  <smtp from="xxxx@yyyy.zzzz">
        <network host="smtp.mandrillapp.com" userName="xxxx" password="xxxx" port="587" />
  </smtp>
  ```    

4. Run "MigrateTask" to ensure an up-to-date-schema
 - From Visual Studio, open Project Properties of your Console Application project, navigate to the "Debug"-tab, and write "MigrateTask" (without quotes) in the multi-line textbox "Command line arguments".
 - Make sure your project is "Set as StartUp Project" and then Start it: CTRL+F5 or F5
 - If the MigrateTask fails, you need to make sure that you have all necessary permissions to the database specified earlier (effectively we're changing the Db schema, and potentially creating a new database - so you need a lot of permission!).
5. Next step is to create a new Class Library project which will contain your actual code/implementations. This is recommended to enforce separation, but it's not required. After creating your new Class Library project, install the following NuGet package to that project:
  ```
  Install-Package Vertica.Integration
  ```
  Finally make sure to add a reference from your Console Application project to this new Class Library project.
6. You're now up and running with the Integration Service. Search the documentation to find examples on how to start using it, e.g. how to Tasks, how to setup custom Migrations, expose HTTP services, setup the Management Portal and much more. Good luck! Remember - any feedback is very much appreciated.

[Back to Table of Contents](#table-of-contents)

## Basics of Tasks

A Task is, in it's simplest form, a .NET class that inherits from **Vertica.Integration.Model.Task**. 
A Task must implement two members:

1. **Description** (Property)
  * Use this to describe the purpose of the task. The Description will be used by the Logging infrastructure as well as the Portal. Make sure to provide a short precise description of the task.
2. **StartTask** (Method)
  * This method will be called when the Task is being executed. This is the place to implement your logic.
  * The method has an **ITaskExecutionContext** argument which provides access to e.g. logging.

**Example of implementing a Task**
  ```c#
using Vertica.Integration.Model;

namespace ClassLibrary2
{
    public class MyFirstTask : Task
    {
        public override string Description
        {
            get { return "This Task is to illustrate how to create tasks. Very meta."; }
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            // Implement your logic here.
            context.Log.Message("This shows how to add messages to the log.");
        }
    }
}
```

In order to be able to execute "MyFirstTask", first you need to register it. There are multiple ways of doing this, the example below demonstrates how to automatically add any (public) simple Tasks (Tasks without Steps - which you will learn about later in this section) that exists in the same assembly as the type defined in the Generic Argument.

```c#
using ClassLibrary2;

namespace ConsoleApplication16
{
    class Program
    {
        static void Main(string[] args)
        {
            IntegrationStartup.Run(args, builder => builder
                .Tasks(tasks => tasks
                    .AddFromAssemblyOfThis<MyFirstTask>()));
        }
    }
}
```

See section [Bootstrapping Tasks](#bootstrapping-tasks) for more examples about bootstrapping tasks.

The Integration Service also offers the possibility to create Tasks where the [Task Execution Flow](#task-execution-flow) can be divided into logical Steps. 
To implement a Task that has Steps, you need to define a class, refered to as a _WorkItem_, which will be passed from the Task to all Steps part of the [Task Execution Flow](#task-execution-flow).

The example below creates a Task "MakeDeploymentTask" with a WorkItem-class named "MakeDeploymentWorkItem". Next three different Steps are created, specifying the same WorkItem-class.

```c#
using System.Collections.Generic;
using Vertica.Integration.Model;

namespace ClassLibrary2
{
    public class MakeDeploymentTask : Task<MakeDeploymentWorkItem>
    {
        public MakeDeploymentTask(IEnumerable<IStep<MakeDeploymentWorkItem>> steps)
            : base(steps)
        {
        }

        public override MakeDeploymentWorkItem Start(ITaskExecutionContext context)
        {
            var workItem = new MakeDeploymentWorkItem();
            workItem.AddFolder("Integration Service");

            return workItem;
        }

        public override void End(MakeDeploymentWorkItem workItem, ITaskExecutionContext context)
        {
            // ... Zip folders and files in WorkItem.
        }

        public override string Description
        {
            get { return "Creates zip-file for deployment of the solution."; }
        }
    }

    public class MakeDeploymentWorkItem
    {
        public void AddFolder(string folder)
        {
            // ...
        }

        public void AddFile(string file)
        {
            // ...
        }
    }

    public class CopyWebsiteArtifacts : Step<MakeDeploymentWorkItem>
    {
        public override void Execute(MakeDeploymentWorkItem workItem, ITaskExecutionContext context)
        {
            workItem.AddFolder("../src/Portal.Website");
        }

        public override string Description
        {
            get { return "TBD"; }
        }
    }

    public class CopySitecoreBaseline : Step<MakeDeploymentWorkItem>
    {
        public override void Execute(MakeDeploymentWorkItem workItem, ITaskExecutionContext context)
        {
            workItem.AddFile("../installs/Sitecore 8.0 rev. 141212.zip");
        }

        public override string Description
        {
            get { return "TBD"; }
        }
    }

    public class CopyUCommerceBaseline : Step<MakeDeploymentWorkItem>
    {
        public override void Execute(MakeDeploymentWorkItem workItem, ITaskExecutionContext context)
        {
            workItem.AddFile("../installs/uCommerce-for-Sitecore-6.6.6.15140.zip");
        }

        public override string Description
        {
            get { return "Copies uCommerce artifacts, including base ."; }
        }
    }
}
```

In order to be able to execute "MakeDeploymentTask" it needs to be registered explicitly. The example below demonstrates how to do just that. The order in which the Steps are registred will be honored by Integration Service.

```c#
using ClassLibrary2;

namespace ConsoleApplication16
{
    class Program
    {
        static void Main(string[] args)
        {
            IntegrationStartup.Run(args, builder => builder
                .Tasks(tasks => tasks
                    .Task<MakeDeploymentTask, MakeDeploymentWorkItem>(task => task
                        .Step<CopyWebsiteArtifacts>()
                        .Step<CopySitecoreBaseline>()
                        .Step<CopyUCommerceBaseline>())));
        }
    }
}
```

Steps will be executed sequentially and as previously mentioned in the exact same sequence/order as they are registered. 
An async flavour _might_ be added later, _if_ there's a demand for it.

### Tasks Behaviour and Lifetime
Tasks are _Singletons_. You should therefore never keep any state within the lifetime of this object. 
To ensure a nice decoupled architecture the Integration Service provides Constructor Injection for any dependency you should need in your Tasks. 
By itself the Integration Service offers a number of services (see section [Built-in Services](#built-in-services)
for more information about this) but you can of course register your own classes to be resolved by the IoC container (read more about this here [How to Register Custom dependencies/services](#how-to-register-custom-dependencies-services)).

[Back to Table of Contents](#table-of-contents)

## Bootstrapping Tasks

All Tasks are registered in the initial configuration/bootstrapping of Integration Service.

This configuration is exposed through the *.Tasks(...)* method:

```c#
using ClassLibrary2;

namespace ConsoleApplication16
{
    class Program
    {
        static void Main(string[] args)
        {
            IntegrationStartup.Run(args, builder => builder
                .Tasks(tasks => tasks
					.XXX
        }
    }
}
```
**NOTE** The example above will not compile. 

From the TasksConfiguration you can:

1. Add specific Task(s)
  * This is mandatory if the Task has Steps
2. Add sipmle Tasks (tasks without Steps) from a specific assembly
  * There's an example a bit up in this documentation where the MakeDeploymentTask is registred.
3. Remove a specific Task
  * Use this if you for some reason don't want a Task to be available
4. Clear
  * Will Clear all Tasks, use this if you want to remove all the Built-in tasks

Just about any combination should supported. If you find a scenario that's not supported, let me know.

### Extension Method

As your project grows, you might end up having a lot of Configuration - especially if you have a lot of Tasks with Steps. You can utilize Extension Methods to begin split up this configuration.

Here are some examples of that:

```c#
IntegrationStartup.Run(args, builder => builder
	.Tasks(tasks => tasks
		.AddFromAssemblyOfThis<MyFirstTask>()
		.Task<MakeDeploymentTask, MakeDeploymentWorkItem>(task => task
			.Step<CopyWebsiteArtifacts>()
			.Step<CopySitecoreBaseline>()
			.Step<CopyUCommerceBaseline>())
		.MonitorTask(task => task
			.IncludeElmah())
		.MaintenanceTask(task => task
			.IncludeElmah())));
```

**First example** below illustrates how to move the entire Tasks configuration to an Extension Method:

```c#
public static class TasksConfigurationExtensions
{
	public static ApplicationConfiguration SetupTasks(this ApplicationConfiguration builder)
	{
		return builder.Tasks(tasks => tasks
			.AddFromAssemblyOfThis<MyFirstTask>()
			.Task<MakeDeploymentTask, MakeDeploymentWorkItem>(task => task
				.Step<CopyWebsiteArtifacts>()
				.Step<CopySitecoreBaseline>()
				.Step<CopyUCommerceBaseline>())
			.MonitorTask(task => task
				.IncludeElmah())
			.MaintenanceTask(task => task
				.IncludeElmah()));
	}
}
```

Which will simplify the overall configuration to this:

```c#
IntegrationStartup.Run(args, builder => builder
	.SetupTasks());
```

**Second example** shows how to move just a single Task registration/configuration to an Extension Method:

```c#
public static class MakeDeploymentTaskConfigurationExtensions
{
	public static TasksConfiguration MakeDeploymentTask(this TasksConfiguration tasks)
	{
		return tasks
			.Task<MakeDeploymentTask, MakeDeploymentWorkItem>(task => task
				.Step<CopyWebsiteArtifacts>()
				.Step<CopySitecoreBaseline>()
				.Step<CopyUCommerceBaseline>());
	}
}
```

Which will simplify the overall configuration to this:

```c#
IntegrationStartup.Run(args, builder => builder
	.Tasks(tasks => tasks
		.MakeDeploymentTask()));
```

And you can of course combine these two examples, e.g. by moving the entire Tasks registration/configuration to an Extension Method (first example), and use another Extension Method for configuring the "MakeDeploymentTask".
  
[Back to Table of Contents](#table-of-contents)

## Built-in Tasks

1. **MonitorTask**
  * Has built-in steps
	* **ExportIntegrationErrorsStep** - exports errors and warnings from the IntegrationDb itself
	* **PingUrlsStep** - performs an http(s) request to a predefined set of URL's
  * Requires registration to be available
    * Use **MonitorTask(...)** extension method on the **TasksConfiguration** instance
  * Can easily be extended with additional Steps to Monitor other parts of the solution
	* E.g. extended by the Elmah-package [Integrating Elmah](#integrating-elmah)
	* See [How to Extend MonitorTask](#how-to-extend-monitortask)
  * Sends out an e-mail with the error/warning messages
  * Use **MonitorConfiguration** for configuration of target groups, recipients and more
	* Also see [Migration of MonitorConfiguration](#migration-of-monitorconfiguration)
  * This is typically scheduled to run every 15. minute
2. **MaintenanceTask**
  * Performs a number of clean-up related tasks, including:
	* **CleanUpIntegrationDbStep** - Deletes entries from Task- and ErrorLog that are older than a predefined period
	* **CleanUpArchivesStep** - Deletes archives that are older than a predefined period
  * Requires registration to be available
    * Use **MaintenanceTask(...)** extension method on the **TasksConfiguration** instance	
  * Can easily be extended with additional Steps to perform maintenance of other parts of the solution
	* E.g. extended by the Elmah-package [Integrating Elmah](#integrating-elmah)
	* See [How to Extend MaintenanceTask](#how-to-extend-maintenancetask)
  * Use **MaintenanceWorkItem** for configuration
	* Also see [Migration of MaintenanceWorkItem](#migration-of-maintenanceconfiguration)
  * This is typically scheduled to run once every day
3. **WriteDocumentationTask**
  * Simple Task that uses the **ITaskFactory** to iterate all registred Tasks. If *-ToFile* argument is passed, the task produces a simple TXT file with all tasks and steps written in that.
	* ```.exe WriteDocumentationTask ToFile```
4. **MigrateTask**
  * Task that internally uses FluentMigrator (https://github.com/schambers/fluentmigrator) to ensure up-to-date schema of the Integration Service.
	* ```.exe MigrateTask```  
  * Can easily be extended with custom Migrations, only requirement is that the VersionInfo table can be stored in a SQL server
	* See [Migrations](#migrations) for much more information about this and examples
	
[Back to Table of Contents](#table-of-contents)

## Task Execution Flow

All Tasks are executed from **ITaskRunner**. 

```c#
namespace Vertica.Integration.Model
{
	public interface ITaskRunner
	{
        TaskExecutionResult Execute(ITask task, params string[] arguments);
	}
}  
```

The flow is:

1. *Execute(task, arguments)*-method is invoked
2. A new **TaskLog** is created and persisted by the **ILogger**
  * If any exceptions are thrown part of the flow, an ErrorLog will be created and associated with the TaskLog
3. *task.Start(...)*-method is invoked
4. ... to be continued

<<LOGGING, CONTINUE WITH/BREAK - VISUAL EXAMPLE>>

[Back to Table of Contents](#table-of-contents)

## Scheduling Tasks

TBD. 
[Back to Table of Contents](#table-of-contents)

## Basics of WebApi

TBD. 
[Back to Table of Contents](#table-of-contents)

## Logging and Exceptions

TBD. 
[Back to Table of Contents](#table-of-contents)

## Command Line Reference

TBD. 
[Back to Table of Contents](#table-of-contents)

## Migrations

### Custom Migrations
<<EXAMPLES OF CUSTOM MIGRATIONS THAT ARE NOT DB RELATED>>
<<EXAMPLES OF CUSTOM MIGRATIONS AGAINST A DB>>
<<EXAMPLES OF CUSTOM MIGRATIONS USING UCOMMERCE MIGRATION>>

### Migration of MonitorConfiguration

Before you can run the **MonitorTask** you need to setup **MonitorConfiguration**. 
If you forget to do this, you'll end up getting warnings like the one below:

*[WARNING] No recipients found for target 'Service'.*

There are multiple ways of setting up the **MonitorConfiguration** object:

 - Manually, edit in the database
 - From the Portal ([Setting up Portal](#setting-up-portal))
 - Or by using Migrations (recommended)

The example below shows how to create a Migration that will setup **MonitorConfiguration**:

```c#
using System;
using FluentMigrator;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Infrastructure.Logging;

namespace ClassLibrary2.Migrations.IntegrationDb
{
    [Migration(1434102243)]
    public class M1434102243_SetupMonitorConfiguration : IntegrationMigration
    {
        public override void Up()
        {
            var configuration = GetConfiguration<MonitorConfiguration>();

            MonitorTarget service = configuration.EnsureMonitorTarget(Target.Service);
            service.Recipients = new[] { "bhk@vertica.dk", "service@vertica.dk" };

            SaveConfiguration(configuration);
        }

        public override void Down()
        {
            throw new NotSupportedException();
        }
    }
}
```

**NOTE** The value "1434102243" represents a Unix Timestamp. This is the preferred way of defining incremental version numbers for Migrations.
Use e.g. http://www.unixtimestamp.com/ to get the current timestamp.

If you create Custom Targets, the following example shows how to setup such in a Migration:

```c#
using System;
using FluentMigrator;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Infrastructure.Logging;

namespace ClassLibrary2.Migrations.IntegrationDb
{
    [Migration(1434115297)]
    public class M1434115297_SetupCustomTargetInMonitorConfiguration : IntegrationMigration
    {
        public override void Up()
        {
            var configuration = GetConfiguration<MonitorConfiguration>();

            MonitorTarget business = configuration.EnsureMonitorTarget(Target.Custom("Business"));
            business.Recipients = new[] { "bhk@vertica.dk", "business@vertica.dk" };

            SaveConfiguration(configuration);
        }

        public override void Down()
        {
            throw new NotSupportedException();
        }
    }
}
```
In the example above a custom target named "Business" is configured. This allows you to log errors/warnings to this group:

```c#
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;

namespace ClassLibrary2
{
    public class LogToBusinessExampleTask : Task
    {
        public override string Description
        {
            get { return "This Task is to illustrate how to log to custom targets."; }
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            context.Log.Warning(Target.Custom("Business"), "Some warning...");
            context.Log.Error(Target.Custom("Business"), "Some error...");
        }
    }
}
```

<<EXAMPLE SUBJECT PREFIX>>
<<EXAMPLE PingUrlsConfiguration>>
<<EXAMPLE LASTRUN>>

[Back to Table of Contents](#table-of-contents)

### Migration of MaintenanceConfiguration

TBD. 
[Back to Table of Contents](#table-of-contents)

## Built-in Services

TBD. 
[Back to Table of Contents](#table-of-contents)

## Configurations

TBD. 
[Back to Table of Contents](#table-of-contents)

## Archives

TBD. 
[Back to Table of Contents](#table-of-contents)

## CSV

TBD. 
[Back to Table of Contents](#table-of-contents)

## FTP

TBD. 
[Back to Table of Contents](#table-of-contents)

## Setting up Portal

Portal is "just" an administration interface on top of the Integration Service. From the Portal you can see various information including Tasks, Logging. From the Portal you can also administer Configurations.

Setting up the Portal is easy.

1. Install via NuGet to the Visual Studio Project hosting Integration Service, typically this is your Console Application (.exe)
  ```
  Install-Package Vertica.Integration.Portal
  ```
2. Invoke the Extension Method *UsePortal()* that effectively initializes the Portal
  ```c#
	using Vertica.Integration.Portal;

	namespace ConsoleApplication16
	{
		class Program
		{
			static void Main(string[] args)
			{
				IntegrationStartup.Run(args, builder => builder
					.UsePortal());
			}
		}
	}
  ```
3. To open up the Portal, run the Integration Service with the following arguments
  ```.exe WebApiTask -url http://localhost:8123```

  ... you can of course choose any Host Name and any Port Number other than localhost:8123 as mentioned above.
4. Open your browser and navigate to http://localhost:8123
  
[Back to Table of Contents](#table-of-contents)

## Integrating Elmah

If you are using Elmah with database logging in your project, we have a package that integrates the Elmah log with the Integration Service.
This allows you to aggregate errors logged by Elmah into the Monitoring e-mail provided by the built-in **MonitorTask**.

**NOTE** Currently Elmah.io is not integrated.

Integration Elmah is easy.

1. Install via NuGet to the Visual Studio Project hosting Integration Service, typically this is your Console Application (.exe)
  ```
  Install-Package Vertica.Integration.Logging.Elmah
  ```
2. Invoke the Extension Method *IncludeElmah()* part of registering **MonitorTask**
  ```c#
	using Vertica.Integration.Domain.Monitoring;
	using Vertica.Integration.Logging.Elmah;

	namespace ConsoleApplication16
	{
		class Program
		{
			static void Main(string[] args)
			{
				IntegrationStartup.Run(args, builder => builder
					.Tasks(tasks => tasks
						.MonitorTask(task => task
							.IncludeElmah())));
			}
		}
	}
  ```
3. Open app.config and add a new ConnectionString to configuration, named "Logging.ElmahDb" (default), with a valid connection to your Elmah SQL database
  ```xml
  <connectionStrings>
      <add name="Logging.ElmahDb" connectionString="Integrated Security=SSPI;Data Source=[NAME-OF-SQL-SERVER];Database=[NAME-OF-ELMAH-DATABASE]" />
  </connectionStrings>  
  ``` 
4. Create a Migration to setup **ElmahConfiguration** if you need to change any default options
5. Execute **MonitorTask** to see it working  
	* ```.exe MonitorTask```

[Back to Table of Contents](#table-of-contents)

## Integrating Azure - BlobStorage

TBD. 
[Back to Table of Contents](#table-of-contents)

## How to Disable IntegrationDb

If you are using Integration Service in a way where the requirement on an underlying database seems overkill. Maybe you're using it to expose some few HTTP services or maybe you're using it as a "Run-Once" Legacy Migration platform.

Disabling the IntegrationDb is easy. Use the **.Database(...)** method on part of bootstrapping Integration Service.

```c#
namespace ConsoleApplication16
{
    class Program
    {
        static void Main(string[] args)
        {
            IntegrationStartup.Run(args, builder => builder
                .Database(database => database.DisableIntegrationDb()));
        }
    }
}
```

**NOTE** If you Disable IntegrationDb you won't be able to use all Built-In services/tasks, but Integration Service will make sure to give you a nice Exception Message if you try to.
 - ```
Unhandled Exception: Vertica.Integration.Infrastructure.Database.Databases.DatabaseDisabledException: IntegrationDb has been disabled.

Examine the DependencyChain below to see which component has a dependency of this:

Component 'Late bound Vertica.Integration.Infrastructure.Database.IDbFactory`1[[Vertica.Integration.Infrastructure.Database.DefaultConnection, Vertica.Integration, Version=1.4.5638.24340, Culture=neutral, PublicKeyToken=null]]' resolved as
dependency of
        component 'Late bound Vertica.Integration.Infrastructure.Database.IDbFactory`1[[Vertica.Integration.Infrastructure.Database.DefaultConnection, Vertica.Integration, Version=1.4.5638.24340, Culture=neutral, PublicKeyToken=null]]' resolved as dependency of
        component 'Late bound Vertica.Integration.Infrastructure.Database.IDbFactory' resolved as dependency of
        component 'Vertica.Integration.Infrastructure.Logging.Loggers.DefaultLogger' resolved as dependency of
        component 'Vertica.Integration.Model.TaskRunner' which is the root component being resolved.
```

**NOTE** You can change Logger to a different implementation, see [How to Change Logger](#how-to-change-logger)

[Back to Table of Contents](#table-of-contents)

## How to Change Logger

TBD. 
[Back to Table of Contents](#table-of-contents)

## How to Register Custom dependencies/services

TBD. 
[Back to Table of Contents](#table-of-contents)

## How to Setup connection to custom database

Adding a Custom Connection to a Custom Database is very easy. First you need a couple of things:

1. You need a ConnectionString to your database
2. You need a Public Class that inherits from **Vertica.Integration.Infrastructure.Database.Connection**
3. You need to register this Custom Connection

```xml
<connectionStrings>
  <add name="CustomDb" connectionString="Integrated Security=SSPI;Data Source=[NAME-OF-SQL-SERVER];Database=[NAME-OF-CUSTOM-DATABASE]" />
</connectionStrings>  
```
Will add a new ConnectionString to configuration, named "CustomDb".

```c#
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database;

namespace ClassLibrary2
{
    public class CustomDb : Connection
    {
        public CustomDb()
            : base(ConnectionString.FromName("CustomDb"))
        {
        }
    }
}
```
Will add a class that exposes the Custom Connection. Note: You can also use **ConnectionString.FromText("...")** to hard-code your connection string.

```c#
namespace ConsoleApplication16
{
    class Program
    {
        static void Main(string[] args)
        {
            IntegrationStartup.Run(args, builder => builder
                .Database(database => database
                    .AddConnection(new CustomDb())));
        }
    }
}
```
Will register "CustomDb" as a Custom Connection for it to be used everywhere. The example below illustrates how to use it from a Task.

```c#
using System.Data;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Model;

namespace ClassLibrary2
{
    public class UsingCustomDbExampleTask : Task
    {
        private readonly IDbFactory<CustomDb> _customDb;

        public UsingCustomDbExampleTask(IDbFactory<CustomDb> customDb)
        {
            _customDb = customDb;
        }

        public override string Description
        {
            get { return "Illustrates how to use CustomDb."; }
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            using (IDbSession session = _customDb.OpenSession())
            using (IDbTransaction transaction = session.BeginTransaction())
            {
                session.Execute("...");
                transaction.Commit();
            }
        }
    }
}
```
Shows how to use the Custom Connection by taking a dependency on **IDbFactory<CustomDb>**. 

###IDbFactory<TConnection>
This factory gives you access to any custom connection you register.

The Factory exposes the underlying IDbConnection but more importantly it allows you to create an **IDbSession** against that connection. The **IDbSession** is a very thin Adapter on top of Dapper (https://github.com/StackExchange/dapper-dot-net), giving you few but powerful options to work against your database. If you need to open up the full capabilities of Dapper, then you can simply use it's extension methods, see example below:

```c#
using (IDbConnection connection = _customDb.GetConnection())
{
	connection.Open();

	using (SqlMapper.GridReader reader = connection.QueryMultiple("SELECT 1; SELECT 'TWO'; SELECT 3.5"))
	{
		var one = reader.Read<int>().Single();
		var two = reader.Read<string>().Single();
		var threeeAndAHalf = reader.Read<decimal>().Single();

		context.Log.Message("{0} - {1} - {2}", one, two, threeeAndAHalf);
	}
}
```


From the **IDbSession** you can also create an **IDbTransaction**-scope. Use the BeginTransaction()-method to create such.

[Back to Table of Contents](#table-of-contents)


## How to Extend MonitorTask

TBD 
[Back to Table of Contents](#table-of-contents)
