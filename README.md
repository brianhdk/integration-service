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
 - [HTTP](#http)
 - [Sending out e-mails](#sending-out-e-mails)
 - [Setting up Portal](#setting-up-portal)
 - [Integrating Elmah](#integrating-elmah)
 - [Integrating Azure Blob Storage](#integrating-azure-blob-storage)
 - [Integrating Azure Service Bus Queue](#integrating-azure-service-bus-queue) 
 - [Integrating Payment Service](#integrating-payment-service)
 - [Integrating RavenDB](#integrating-ravendb)
 - [Integrating MongoDB](#integrating-mongodb)
 - [Integrating SQLite](#integrating-sqlite) 
 - [Integrating Perfion PIM](#integrating-perfion-pim)  
 - [Integrating Hangfire](#integrating-hangfire)  
 - [How to Disable IntegrationDb](#how-to-disable-integrationdb)
 - [How to Change Logger](#how-to-change-logger) 
 - [How to Register Custom dependencies/services](#how-to-register-custom-dependenciesservices)
 - [How to Setup connection to a custom database](#how-to-setup-connection-to-a-custom-database) 
 - [How to Setup MonitorFoldersStep](#how-to-setup-monitorfoldersstep)
 - [How to Extend MonitorTask](#how-to-extend-monitortask)
 - [How to Setup ArchiveFoldersStep](#how-to-setup-archivefoldersstep)
 - [How to Extend MaintenanceTask](#how-to-extend-maintenancetask)

## How to Get Started

1. Choosing a Host for Integration Service.

	Typically Integration Service is hosted through a simple .NET Console Application (.exe). Add a new "Console Application" project to your existing (or new solution). 

  **NOTE:** Later you'll add a Class Library project where all your actual implementation code will be placed.

2. Install Integration Service via NuGet

	**NOTE:** Make sure that you have a NuGet Package Source configured for URL: http://nuget.vertica.dk/nuget
	
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
  *By default Integration Service requires a database - but this can be disabled. See section [How to Disable IntegrationDb](#how-to-disable-integrationdb) to read more about the option of running Integration Service without a database.*
  
  ```xml
  <connectionStrings>
      <add name="IntegrationDb" connectionString="Integrated Security=SSPI;Data Source=[NAME-OF-SQL-SERVER];Database=[NAME-OF-INTEGRATION-DATABASE]" />
  </connectionStrings>  
  ```

  Azure example:
  ```xml
  <add name="IntegrationDb" connectionString="Server=tcp:xxxx.database.windows.net,1433;Database=IntegrationDb;User ID=xxxx@xxxx;Password=xxxx;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" />
  ```  

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

  Vertica example:
  ```xml
  <smtp from="your-email@vertica.dk">
        <network host="mail01.vertica.dk" />
  </smtp>
  ```    
  
4. Run **MigrateTask** to ensure an up-to-date-schema
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
            IntegrationStartup.Run(args, application => application
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
            IntegrationStartup.Run(args, application => application
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

If your _WorkItem_ implements IDisposable Integration Service will make sure to call the Dispose()-method even if methods on Steps fail or if Task.End() fails.

### Tasks Behaviour and Lifetime
Tasks are _Singletons_. You should therefore never keep any state within the lifetime of this object. 
To ensure a nice decoupled architecture the Integration Service provides Constructor Injection for any dependency you should need in your Tasks. 
By itself the Integration Service offers a number of services (see section [Built-in Services](#built-in-services)
for more information about this) but you can of course register your own classes to be resolved by the IoC container (read more about this here [How to Register Custom dependencies/services](#how-to-register-custom-dependenciesservices)).

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
            IntegrationStartup.Run(args, application => application
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
IntegrationStartup.Run(args, application => application
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
IntegrationStartup.Run(args, application => application
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
IntegrationStartup.Run(args, application => application
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
	* **MonitorFoldersStep** - monitors a set of configured folders (MonitorConfiguration).
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
  * If an exception is thrown part of the flow, an ErrorLog will be created and associated with the TaskLog - and the flow is aborted
3. *task.Start(...)*-method is invoked and a specific *WorkItem* instance is created (if *task* has Steps)
4. If *task* has Steps, this will happen:
  * In the order Steps are registred, they will sequentially be executed:
    * Steps will first be asked if they require execution: 
	   * *step.ContinueWith(workItem)* will be called
	     * **Execution.StepOut** will break the execution of all steps
		 * **Execution.StepOver** will skip this particular step but continue execution
		 * **Execution.Execute** will continue executing the step
    * If Step require execution (**Execution.Execute**) *step.Execute(workItem, ...)* will be called
5. *task.End(workItem, ...)* will be called at the end of the flow

As stated earlier, if an exception is thrown, this will abort the flow and associate the error with the specific TaskLog. Later this error will be picked up by the **MonitorTask** and e-mailed to a configurable recipient.

[Back to Table of Contents](#table-of-contents)

## Scheduling Tasks

TBD. 
[Back to Table of Contents](#table-of-contents)

## Basics of WebApi

```
Install-Package Vertica.Integration.WebApi
```

.exe WebApiHost url:http://localhost:8123 -service:install
.exe WebApiHost url:http://+:8080/ -service:install

If you leave out the "url" argument, it will use the one specified in app.config: <add key="WebApiHost.DefaultUrl" value="http://localhost:8400" />

.exe WebApiHost -service:install -startmode:Automatic
.exe WebApiHost -service:install -startmode:Automatic -account:NetworkService
.exe WebApiHost -service:install -startmode:Automatic -username:[user] -password:[password]

[Back to Table of Contents](#table-of-contents)

## Logging and Exceptions

TBD. 
[Back to Table of Contents](#table-of-contents)

## Command Line Reference

TBD. 
[Back to Table of Contents](#table-of-contents)

## Migrations

Migrations is a first-class citizen in the Integration Service. Integration Service uses FluentMigrator (https://github.com/schambers/fluentmigrator) internally to ensure it's own database schema.

If you upgrade Integration Service, make sure to run the Task:
```.exe MigrateTask```

The Migration API is open for extensions, this means that you easily can apply your own custom migrations against any database.

### Custom Migrations

To configure Custom Migrations to be executed by the **MigrateTask**, you need to register this part when bootstrapping Integration Service.

This example shows how to apply custom migrations to the IntegrationDb itself. The Migration will create a new table named "CustomTable" and later from a new task named "UseCustomTableTask" we'll interact with this new table.

```c#
using ConsoleApplication16.Migrations.IntegrationDb;

namespace ConsoleApplication16
{
    class Program
    {
        static void Main(string[] args)
        {
            IntegrationStartup.Run(args, application => application
                .Migration(migration => migration
                    .AddFromNamespaceOfThis<M1434483770_AddCustomTableToIntegrationDb>())
                .Tasks(tasks => tasks
                    .Task<UseCustomTableTask>()));
        }
    }
}
```

The code above will ensure that all public classes in the same namespace as "M1434483770_AddCustomTableToIntegrationDb" will have the *Up()*-method invoked, if the migration has not already been executed previously. 

```c#
using System;
using FluentMigrator;

namespace ConsoleApplication16.Migrations.IntegrationDb
{
    [Migration(1434483770)]
    public class M1434483770_AddCustomTableToIntegrationDb : Migration
    {
        public override void Up()
        {
            Create.Table("CustomTable")
                .WithColumn("ID").AsInt32().PrimaryKey().Identity()
                .WithColumn("Name").AsString(255).NotNullable();
        }

        public override void Down()
        {
            throw new NotSupportedException();
        }
    }
}
```

The code above shows the actual implementation of the migration.

**NOTE** The value "1434483770" represents a Unix Timestamp. This is the preferred way of defining incremental version numbers for Migrations.
Use e.g. http://www.unixtimestamp.com/ to get the current timestamp.

As mentioned previously, migrations are applied by running the **MigrateTask**

```.exe MigrateTask```

```c#
using System.Data;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Model;

namespace ConsoleApplication16
{
    public class UseCustomTableTask : Task
    {
        private readonly IDbFactory _integrationDb;

        public UseCustomTableTask(IDbFactory integrationDb)
        {
            _integrationDb = integrationDb;
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            using (IDbSession session = _integrationDb.OpenSession())
            using (IDbTransaction transaction = session.BeginTransaction())
            {
                int recordId = session.ExecuteScalar<int>(@"
INSERT INTO CustomTable (Name) VALUES ('John Doe');
SELECT SCOPE_IDENTITY();");

                context.Log.Message("Inserted record: {0}", recordId);

                transaction.Commit();
            }
        }

        public override string Description
        {
            get { return "Illustrates how to use newly created Custom Table"; }
        }
    }
}
```

Finally the code above shows how to interact with the newly created table in the IntegrationDb.

It's important to mention again that you can run migrations against any SQL database in your solution - and that migrations can be used for more than "just" updating a database schema - this will be covered by examples below. The recommendation is to run the ```.exe MigrateTask``` after each deployment to stage/production environments.

### Custom Database migrations

This example illustrates how to create custom migrations against another SQL Server database than the IntegrationDb itself.


```c#
using ClassLibrary2;
using ConsoleApplication16.Migrations.CustomDb;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database.Migrations;

namespace ConsoleApplication16
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectionString customDb = ConnectionString.FromName("CustomDb");

            IntegrationStartup.Run(args, application => application
                .Database(database => database
                    .AddConnection(new CustomDb(customDb)))
                .Migration(migration => migration
                    .AddFromNamespaceOfThis<M1434484967_AddCustomTableToCustomDb>(DatabaseServer.SqlServer2014, customDb))
                .Tasks(tasks => tasks
                    .Task<UseCustomTableTask>()));
        }
    }
}
```

The code above registers all public classes in the same namespace as "M1434484967_AddCustomTableToCustomDb" to have their *Up()*-method invoked against a SQL Server referenced from the **ConnectionString** instance *customDb*.

The code also creates a custom database connection. See [How to Setup connection to a custom database](#how-to-setup-connection-to-a-custom-database) for more information about this topic.

```c#
using System;
using FluentMigrator;

namespace ConsoleApplication16.Migrations.CustomDb
{
    [Migration(1434484967)]
    public class M1434484967_AddCustomTableToCustomDb : Migration
    {
        public override void Up()
        {
            Create.Table("CustomTable")
                .WithColumn("ID").AsInt32().PrimaryKey().Identity()
                .WithColumn("Name").AsString(255).NotNullable();
        }

        public override void Down()
        {
            throw new NotSupportedException();
        }
    }
}
```

The code above shows the actual implementation of the migration.

**NOTE** The value "1434484967" represents a Unix Timestamp. This is the preferred way of defining incremental version numbers for Migrations.
Use e.g. http://www.unixtimestamp.com/ to get the current timestamp.

As mentioned previously, migrations are applied by running the **MigrateTask**

```.exe MigrateTask```

```c#
using System.Data;
using ClassLibrary2;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Model;

namespace ConsoleApplication16
{
    public class UseCustomTableTask : Task
    {
        private readonly IDbFactory<CustomDb> _customDb;

        public UseCustomTableTask(IDbFactory<CustomDb> customDb)
        {
            _customDb = customDb;
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            using (IDbSession session = _customDb.OpenSession())
            using (IDbTransaction transaction = session.BeginTransaction())
            {
                int recordId = session.ExecuteScalar<int>(@"
INSERT INTO CustomTable (Name) VALUES ('John Doe');
SELECT SCOPE_IDENTITY();");

                context.Log.Message("Inserted record: {0}", recordId);

                transaction.Commit();
            }
        }

        public override string Description
        {
            get { return "Illustrates how to use newly created Custom Table"; }
        }
    }
}
```

Finally the code above shows how to interact with the newly created table.

### Non DB related Migration - Running a Task

This example shows that you can custom migrations are not always about updating a database schema. This simple migration illustrates how to run a specific task. This is useful if you need to run a specific task after a deployment to stage/production environment. E.g. if you need to re-import and re-index the catalog. 

```c#
using ConsoleApplication16.Migrations.IntegrationDb;

namespace ConsoleApplication16
{
    class Program
    {
        static void Main(string[] args)
        {
            IntegrationStartup.Run(args, application => application
                .Migration(migration => migration
                    .AddFromNamespaceOfThis<M1434488770_RunImportCatalogTask>())
                .Tasks(tasks => tasks
                    .AddFromAssemblyOfThis<ImportCatalogTask>()));
        }
    }
}
```

The custom migration inherits from the abstract class **IntegrationMigration** which provides some useful methods, e.g. the *RunTask<TTask>()*-method


```c#
using System;
using FluentMigrator;
using Vertica.Integration.Infrastructure.Database.Migrations;

namespace ConsoleApplication16.Migrations.IntegrationDb
{
    [Migration(1434488770)]
    public class M1434488770_RunImportCatalogTask : IntegrationMigration
    {
        public override void Up()
        {
            RunTask<ImportCatalogTask>();
        }

        public override void Down()
        {
            throw new NotSupportedException();
        }
    }
}
```

**NOTE** The value "1434484967" represents a Unix Timestamp. This is the preferred way of defining incremental version numbers for Migrations.
Use e.g. http://www.unixtimestamp.com/ to get the current timestamp.

```c#
using Vertica.Integration.Model;

namespace ConsoleApplication16
{
    public class IndexCatalogTask : Task
    {
        public override void StartTask(ITaskExecutionContext context)
        {
            // ... Index search catalog
        }

        public override string Description
        {
            get { return "Index search catalog."; }
        }
    }

    public class ImportCatalogTask : Task
    {
        private readonly ITaskFactory _factory;
        private readonly ITaskRunner _runner;

        public ImportCatalogTask(ITaskFactory factory, ITaskRunner runner)
        {
            _factory = factory;
            _runner = runner;
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            // ... Import catalog

            // after importing catalog, re-indexing should happen
            _runner.Execute(_factory.Get<IndexCatalogTask>());
        }

        public override string Description
        {
            get { return "Import catalog and runs the IndexCatalog task."; }
        }
    }
}
```

### uCommerce migration

TBD

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

There are a couple of helpers inside the project for working with CSV files both for reading and writing purposes.

### Reading CSV

The built-in *ICsvParser* is useful when reading CSV. Below is a simple example on how to use it:

```c#
using System.IO;
using System.Linq;
using Vertica.Integration.Infrastructure.Parsing;
using Vertica.Integration.Model;

namespace ConsoleApplication16
{
	public class ReadCsvFileDemoTask : Task
	{
		private readonly ICsvParser _csvParser;

		public ReadCsvFileDemoTask(ICsvParser csvParser)
		{
			_csvParser = csvParser;
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			// Reads a file with headers, outputting column "Name"
			using (FileStream stream = File.OpenRead(@"C:\tmp\file-with-names.csv"))
			{
				CsvRow[] rows = _csvParser.Parse(stream).ToArray();

				foreach (CsvRow row in rows)
					context.Log.Message(row["Name"]);
			}

			// Reads a file with no headers and with a non-standard delimiter, outputting first column
			using (FileStream stream = File.OpenRead(@"C:\tmp\odd-file.csv"))
			{
				CsvRow[] rows = _csvParser.Parse(stream, csv => csv.NoHeaders().ChangeDelimiter("|")).ToArray();

				foreach (CsvRow row in rows)
					context.Log.Message("{0}", row[0]);
			}
		}

		public override string Description
		{
			get { return "TBD"; }
		}
	}
}
```

### CsvRow type

The *CsvRow*-type represents a single CSV row. Use this to get/set the value of a single column, either by name or by index.

You can also read meta-data like headers and line number from a *CsvRow*.

### Writing CSV

On the *CsvRow*-type there is a static method *BeginRows(...)* which helps you to write CSV. See the examples below:

```c#
using System;
using Vertica.Integration.Infrastructure.Parsing;
using Vertica.Integration.Model;

namespace ConsoleApplication16
{
	public class WriteCsvDemoTask : Task
	{
		public override void StartTask(ITaskExecutionContext context)
		{
			// Create CSV with header
			string csv1 = CsvRow.BeginRows("Name", "Description")
				.Configure(x => x.ReturnHeaderAsRow())
				.Add("Name A", "Description A")
				.Add("Name B", "Description B")
				.ToString();

			context.Log.Message(csv1);

			// Create CSV without header - no validation of entered data
			string csv2 = CsvRow.BeginRows()
				.Add("Name A", "Description A")
				.Add("Something", "Very", "Different")
				.ToString();

			context.Log.Message(csv2);

			// Create CSV using a mapper which helps ensuring a valid schema
			string csv3 = CsvRow.BeginRows("Name", "Description")
				.AddUsingMapper(mapper => mapper
					.Map("Description", "Description A")
					.Map("Name", "Name A"))
				.ToString();

			context.Log.Message(csv3);

			// Create CSV from a collection of elements using the mapper
			string csv4 = CsvRow.BeginRows("Name", "Description")
				.Configure(x => x.ReturnHeaderAsRow())
				.FromUsingMapper(new[] { "A", "B", "C" }, (mapper, data) => mapper
					.Map("Name", String.Format("Name {0}", data))
					.Map("Description", String.Format("Description {0}", data)))
				.ToString();

			context.Log.Message(csv4);
		}

		public override string Description
		{
			get { return "TBD"; }
		}
	}
}
```

### QueryToCsv extension method
TODO

[Back to Table of Contents](#table-of-contents)

## FTP

There is a built-in client for FTP - here are the steps to get you started:

1. Create a constructor dependency on *IFtpClientFactory*. 
2. Use the *Create(...)* method to have an instance of *IFtpClient* created.

### Example

This example shows a simple *Task* that downloads CSV files to a local drive:

```c#
using System.IO;
using Vertica.Integration.Infrastructure.Parsing;
using Vertica.Integration.Infrastructure.Remote;
using Vertica.Integration.Infrastructure.Remote.Ftp;
using Vertica.Integration.Model;

namespace ConsoleApplication16
{
	public class FtpClientDemoTask : Task
	{
		private readonly IFtpClientFactory _ftpClientFactory;

		public FtpClientDemoTask(IFtpClientFactory ftpClientFactory)
		{
			_ftpClientFactory = ftpClientFactory;
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			IFtpClient client = _ftpClientFactory.Create("ftp://ftp.vertica.dk/BHK", ftp => ftp
				.Credentials("GuestFTP", "VerticaPass1010"));

			// Enter directory named APMM_SOCONF
			client.NavigateDown("APMM_SOCONF");

			// Navigate all CSV files in directory
			foreach (string csvFile in client.ListDirectory(x => x.EndsWith(".csv")))
			{
				// Download file to local directory
				client.DownloadToLocal(csvFile, new DirectoryInfo(@"c:\tmp\"));
			}
		}

		public override string Description
		{
			get { return "TBD"; }
		}
	}
}
```

[Back to Table of Contents](#table-of-contents)

## HTTP

Integration Service has a built-in factory that makes it easy to perform HTTP-requests. 

1. Create a constructor dependency on *IHttpClientFactory*. 
2. Use the *Create()* method to have an instance of *HttpClient* created.
3. Optionally install the "Microsoft.AspNet.WebApi.Client" NuGet package for a richer API:
  
  ```
  Install-Package Microsoft.AspNet.WebApi.Client
  ```

### Example

This example shows a simple *Task* that performs an HTTP POST to a given URL utilizing the *Microsoft.AspNet.WebApi.Client* library.

```c#
using System.Net.Http;
using Vertica.Integration.Infrastructure.Remote;
using Vertica.Integration.Model;

namespace ConsoleApplication16
{
	public class HttpClientDemoTask : Task
	{
		private readonly IHttpClientFactory _httpClientFactory;

		public HttpClientDemoTask(IHttpClientFactory httpClientFactory)
		{
			_httpClientFactory = httpClientFactory;
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			using (HttpClient httpClient = _httpClientFactory.Create())
			{
				var request = new Request();

				HttpResponseMessage httpResponse = 
					httpClient.PutAsJsonAsync("http://localhost:8123/Service", request)
						.Result;

				httpResponse.EnsureSuccessStatusCode();

				Response response = 
					httpResponse.Content.ReadAsAsync<Response>()
						.Result;

				context.Log.Message("Got response: {0}", response);
			}
		}

		public override string Description
		{
			get { return "Example of Task using the HttpClient."; }
		}
	}

	public class Request
	{
		// Any serializable properties goes here...
	}

	public class Response
	{
		// Any serializable properties goes here...
	}
}
```

[Back to Table of Contents](#table-of-contents)

## Sending out e-mails


Integration Service has a built-in service *IEmailService* to send out e-mails. 
The default implementation of *IEmailService* uses the SMTP configuration, so before you use it, [make sure SMTP is configured correct](#smtp).

*IEmailService* has a *Send(...)* method, which requires an instance of a class that inherits from the abstract class *EmailTemplate*. 

You can easily implement your own classes inheriting from *EmailTemplate* but in most cases the built-in *TextBasedEmailTemplate* is sufficient. Below is an example of a Task that sends an e-mail using the *TextBasedEmailTemplate* class:

```c#
using Vertica.Integration.Infrastructure.Email;
using Vertica.Integration.Model;

namespace ConsoleApplication16
{
	public class EmailDemoTask : Task
	{
		private readonly IEmailService _emailService;

		public EmailDemoTask(IEmailService emailService)
		{
			_emailService = emailService;
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			var email = new TextBasedEmailTemplate("Some nice subject")
				.WriteLine("This is the first line in this text-based e-mail.")
				.WriteLine("This is the second line in the e-mail. Notice the fluent-interface.");

			_emailService.Send(email, "bhk@vertica.dk");
		}

		public override string Description
		{
			get { return "TBD"; }
		}
	}
}
```

If you need to attach files to the e-mail, this can also easily be done, see this example below:

```c#
using System;
using System.IO;
using System.Net.Http;
using System.Net.Mail;
using Vertica.Integration.Infrastructure.Email;
using Vertica.Integration.Infrastructure.Remote;
using Vertica.Integration.Model;

namespace ConsoleApplication16
{
	public class EmailWithAttachmentDemoTask : Task
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IEmailService _emailService;

		public EmailWithAttachmentDemoTask(IEmailService emailService, IHttpClientFactory httpClientFactory)
		{
			_emailService = emailService;
			_httpClientFactory = httpClientFactory;
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			using (HttpClient client = _httpClientFactory.Create())
			using (Stream imageStream = client.GetStreamAsync("https://dl.dropboxusercontent.com/u/4142207/never-delete.jpg").Result)
			{
				var email = new TextBasedEmailTemplate("Annual report {0}", DateTime.Now.Year)
					.WriteLine("Please see attached the annual report for {0}.", DateTime.Now.Year)
					.AddAttachment(new Attachment(@"c:\tmp\annual-report.pdf")) // Existing file
					.AddAttachment(new Attachment(imageStream, "image.jpg"));

				_emailService.Send(email, "bhk@vertica.dk");
			}
		}

		public override string Description
		{
			get { return "TBD"; }
		}
	}
}
```

The example above shows how you can use the method *AddAttachment(...)* to include attachments. From there on, it's System.Net.Mail API that takes over handling these attachments.
It also shows how you can attach something that is not just a simple existing file on the file system, but in this case a Stream from a HTTP Request containing an image.

### Attach a Blob from Azure Storage
If you find your self in need of creating attachments directly from blobs in Azure Storage, here's how you do that:

```c#
CloudBlobClient client = _blobStorageClientFactory.Create();
CloudBlobContainer container = client.GetContainerReference("mycontainer");
CloudBlockBlob blob = container.GetBlockBlobReference("blob-id.jpg");

using (var memoryStream = new MemoryStream())
{
	blob.DownloadToStream(memoryStream);
	memoryStream.Position = 0;

	var email = new TextBasedEmailTemplate("Some subject")
		.AddAttachment(new Attachment(memoryStream, blob.Name));
		
	_emailService.Send(email, "bhk@vertica.dk");
}
```

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
			IntegrationStartup.Run(args, application => application
				.UsePortal());
		}
	}
}
  ```
3. To open up the Portal, run the Integration Service with the following arguments:

  ```
  .exe WebApiTask -url http://localhost:8123
  ```

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
			IntegrationStartup.Run(args, application => application
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

## Integrating Azure Blob Storage

You can integrate to Azure Blob Storage by installing the package below:

```
Install-Package Vertica.Integration.Azure
```

This allows you to create one or more connections to a Storage account on Azure.

The example below shows you how to configure two connections and how to use them from a Task:

```c#
using Microsoft.WindowsAzure.Storage.Blob;
using Vertica.Integration.Azure;
using Vertica.Integration.Azure.Infrastructure.BlobStorage;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Model;

namespace ConsoleApplication16
{
    class Program
    {
        static void Main(string[] args)
        {
			IntegrationStartup.Run(args, application => application
				.UseAzure(azure => azure
					.BlobStorage(blobStorage => blobStorage
						.DefaultConnection(ConnectionString.FromText("DefaultEndpointsProtocol=https;AccountName=integrationservice;AccountKey=xyz"))
						.AddConnection(new SecondaryAccount()))));
        }
    }

	public class SecondaryAccount : Connection
	{
		public SecondaryAccount() 
			: base(ConnectionString.FromName("AnotherBlobStorageAccount"))
		{
		}
	}

	public class BlobStorageDemoTask : Task
	{
		private readonly IAzureBlobStorageClientFactory _defaultClientFactory;
		private readonly IAzureBlobStorageClientFactory<SecondaryAccount> _secondaryClientFactory;

		public BlobStorageDemoTask(IAzureBlobStorageClientFactory defaultClientFactory, IAzureBlobStorageClientFactory<SecondaryAccount> secondaryClientFactory)
		{
			_defaultClientFactory = defaultClientFactory;
			_secondaryClientFactory = secondaryClientFactory;
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			CloudBlobClient defaultClient = _defaultClientFactory.Create();
			CloudBlobContainer defaultContainer = defaultClient.GetContainerReference("container");
			CloudBlockBlob defaultFileBlob = defaultContainer.GetBlockBlobReference("file.jpg");

			CloudBlobClient secondaryClient = _secondaryClientFactory.Create();
			CloudBlobContainer secondaryContainer = secondaryClient.GetContainerReference("container");
			CloudBlockBlob secondaryFileBlob = secondaryContainer.GetBlockBlobReference("file.jpg");
		}

		public override string Description
		{
			get { return "TBD"; }
		}
	}
}
```

[Back to Table of Contents](#table-of-contents)

## Integrating Azure Service Bus Queue

You can integrate to Azure Service Bus Queue by installing the package below:

```
Install-Package Vertica.Integration.Azure
```

This allows you to create one or more connections to a Service Bus account on Azure.

The example below shows you how to configure two connections and how to use them from a Task:

```c#
using Microsoft.ServiceBus.Messaging;
using Vertica.Integration.Azure;
using Vertica.Integration.Azure.Infrastructure.ServiceBus;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Model;

namespace ConsoleApplication16
{
    class Program
    {
        static void Main(string[] args)
        {
			IntegrationStartup.Run(args, application => application
				.UseAzure(azure => azure
					.ServiceBus(serviceBus => serviceBus
						.DefaultConnection(ConnectionString.FromText("Endpoint=sb://integration-service.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=xyz"))
						.AddConnection(new SecondaryAccount()))));
        }
    }

	public class SecondaryAccount : Connection
	{
		public SecondaryAccount() 
			: base(ConnectionString.FromName("AnotherServiceBusAccount"))
		{
		}
	}

	public class BlobStorageDemoTask : Task
	{
		private readonly IAzureServiceBusClientFactory _defaultClientFactory;
		private readonly IAzureServiceBusClientFactory<SecondaryAccount> _secondaryClientFactory;

		public BlobStorageDemoTask(IAzureServiceBusClientFactory defaultClientFactory, IAzureServiceBusClientFactory<SecondaryAccount> secondaryClientFactory)
		{
			_defaultClientFactory = defaultClientFactory;
			_secondaryClientFactory = secondaryClientFactory;
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			QueueClient defaultQueueClient = _defaultClientFactory.CreateQueueClient("queueName");
			defaultQueueClient.Send(new BrokeredMessage("Some message"));

			QueueClient secondaryQueueClient = _secondaryClientFactory.CreateQueueClient("queueName");
			BrokeredMessage message = secondaryQueueClient.Receive();

			try
			{
				string body = message.GetBody<string>();
				message.Complete();
			}
			catch
			{
				message.Abandon();
				throw;
			}
		}

		public override string Description
		{
			get { return "TBD"; }
		}
	}
}
```

## Integrating Payment Service

TBD. 
[Back to Table of Contents](#table-of-contents)

## Integrating RavenDB

You can integrate to RavenDB by installing the package below:

```
Install-Package Vertica.Integration.RavenDB
```

This allows you to have connections to one or more RavenDB databases.

The example below shows you how to create two connections and how to use them from a Task:

```c#
using Raven.Client;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Model;
using Vertica.Integration.RavenDB;
using Vertica.Integration.RavenDB.Infrastructure;

namespace ConsoleApplication16
{
    class Program
    {
        static void Main(string[] args)
        {
			IntegrationStartup.Run(args, application => application
				.UseRavenDb(ravenDb => ravenDb
					.DefaultConnection(ConnectionString.FromText("Url = http://localhost:8998; Database = MyRavenDb"))
					.AddConnection(new SecondRavenDb())));
        }
    }

	// This connection will be served when using the generic IRavenDbFactory<SecondRavenDb> interface.
	public class SecondRavenDb : Connection
	{
		public SecondRavenDb()
			: base(ConnectionString.FromName("NameFromConnectionStringElement"))
		{
		}

		protected override IDocumentStore Create(IKernel kernel)
		{
			// Control how the IDocumentStore is created here.
			return base.Create(kernel);
		}

		protected override void Initialize(IDocumentStore documentStore, IKernel kernel)
		{
			base.Initialize(documentStore, kernel);

			// Add custom initialization here.
		}
	}

	public class RavenDbDemoTask : Task
	{
		private readonly IRavenDbFactory _ravenDb;
		private readonly IRavenDbFactory<SecondRavenDb> _secondRavenDb;

		public RavenDbDemoTask(IRavenDbFactory ravenDb, IRavenDbFactory<SecondRavenDb> secondRavenDb)
		{
			_ravenDb = ravenDb;
			_secondRavenDb = secondRavenDb;
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			using (IDocumentSession session = _ravenDb.DocumentStore.OpenSession())
			{
				// do anything here...
			}

			IDocumentStore documentStore = _secondRavenDb.DocumentStore;
		}

		public override string Description
		{
			get { return "TBD"; }
		}
	}
}
```

[Back to Table of Contents](#table-of-contents)

## Integrating MongoDB

You can integrate to MongoDB by installing the package below:

```
Install-Package Vertica.Integration.MongoDB
```

This allows you to have connections to one or more MongoDB databases.

The example below shows you how to create two connections and how to use them from a Task:

```c#
using System;
using MongoDB.Driver;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Model;
using Vertica.Integration.MongoDB;
using Vertica.Integration.MongoDB.Infrastructure;

namespace ConsoleApplication16
{
    class Program
    {
        static void Main(string[] args)
        {
			IntegrationStartup.Run(args, application => application
				.UseMongoDb(mongoDb => mongoDb
					.DefaultConnection(ConnectionString.FromText("mongodb://user:password@ds036178.mongolab.com:36178/project"))
					.AddConnection(new SecondMongoDb())));
        }
    }

	// This connection will be served when using the generic IMongoDbClientFactory<SecondMongoDb> interface.
	public class SecondMongoDb : Connection
	{
		public SecondMongoDb()
			: base(ConnectionString.FromName("NameFromConnectionStringElement"))
		{
		}
	}

	public class MongoDbDemoTask : Task
	{
		private readonly IMongoDbClientFactory _mongoDb;
		private readonly IMongoDbClientFactory<SecondMongoDb> _secondMongoDb;

		public MongoDbDemoTask(IMongoDbClientFactory mongoDb, IMongoDbClientFactory<SecondMongoDb> secondMongoDb)
		{
			_mongoDb = mongoDb;
			_secondMongoDb = secondMongoDb;
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			IMongoCollection<SomeDocumentDto> documents =
				_mongoDb.Database.GetCollection<SomeDocumentDto>("myDocuments");

			// find specific document
			SomeDocumentDto document = documents
				.Find(x => x.Id == Guid.Parse("808F7DC5-B98E-4B53-801C-2C8FD3730AC4"))
				.SingleOrDefaultAsync().Result;

			IMongoClient client = _secondMongoDb.Client;
		}

		public override string Description
		{
			get { return "TBD"; }
		}
	}

	public class SomeDocumentDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
	}
}
```

[Back to Table of Contents](#table-of-contents)

## Integrating SQLite

You can integrate to SQLite by installing the package below:

```
Install-Package Vertica.Integration.SQLite
```

This allows you to have connections to one or more SQLite databases.

The example below shows you how to create a connection and how to use it from a Task:

```c#
using ConsoleApplication16.Migrations.SQLite;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Model;
using Vertica.Integration.SQLite;

namespace ConsoleApplication16
{
    class Program
    {
        static void Main(string[] args)
        {
	        var sqliteConnection = new MyConnection("myDb.sqlite");

			IntegrationStartup.Run(args, application => application
				.Database(database => database
					.SQLite(sqlite => sqlite.AddConnection(sqliteConnection)))
				.Migration(migration => migration
					.AddFromNamespaceOfThis<M1_SQLiteMigration>(DatabaseServer.Sqlite, sqliteConnection.ConnectionString))
				.Tasks(tasks => tasks.Task<MyTask>()));
        }
    }

	public class MyConnection : SQLiteConnection
	{
		public MyConnection(string fileName)
			: base(FromCurrentDirectory(fileName))
		{
		}
	}

	public class MyTask : Task
	{
		private readonly ITaskFactory _taskFactory;
		private readonly ITaskRunner _taskRunner;
		private readonly IDbFactory<MyConnection> _myConnection;

		public MyTask(IDbFactory<MyConnection> myConnection, ITaskFactory taskFactory, ITaskRunner taskRunner)
		{
			_myConnection = myConnection;
			_taskFactory = taskFactory;
			_taskRunner = taskRunner;
		}

		public override string Description
		{
			get { return "TBD"; }
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			// Execute migrations - to ensure the database tables are created
			//  - normally you'll not do this from a Task, but part of your deployment.
			_taskRunner.Execute(_taskFactory.Get<MigrateTask>());

			using (var session = _myConnection.OpenSession())
			{
				int id = session.ExecuteScalar<int>(@"
INSERT INTO MyTable (Name) VALUES (@Name);
SELECT last_insert_rowid();", new { Name = "Test" });

				context.Log.Message("Number of rows: {0}", id);
			}
		}
	}
}
```

Below is the Migration class used in the example above.

```c#
using System;
using FluentMigrator;

namespace ConsoleApplication16.Migrations.SQLite
{
	[Migration(1)]
	public class M1_SQLiteMigration : Migration
	{
		public override void Up()
		{
			Create.Table("MyTable")
				.WithColumn("ID").AsInt32().PrimaryKey().Identity()
				.WithColumn("Name").AsString().NotNullable();
		}

		public override void Down()
		{
			throw new NotSupportedException();
		}
	}
}
```

[Back to Table of Contents](#table-of-contents)

## Integrating Perfion PIM

To setup a Perfion integration, start by adding the following package:

  ```
  Install-Package Vertica.Integration.Perfion
  ```

The example below illustrates some of the features you can use when working with this Perfion integration, including how to download files/images.

```c#
using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Model;
using Vertica.Integration.Perfion;
using Vertica.Utilities_v4.Collections;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace ConsoleApplication16
{
    class Program
    {
        static void Main(string[] args)
        {
			IntegrationStartup.Run(args, application => application
				.Tasks(tasks => tasks.Task<PerfionTask>())
				.UsePerfion(perfion => perfion
					// Hard-coded connection string (NOT RECOMMENDED)
					.Change(x => x.ConnectionString = ConnectionString.FromText("http://perfion-api.local/Perfion/GetData.asmx"))

					//// Based on element in app.config: <add name="Perfion" connectionString="http://perfion-api.local/Perfion/GetData.asmx" />
					//.Change(x => x.ConnectionString = ConnectionString.FromName("Perfion"))

					// ... or just simply add a connection-string element in app.config, name it "Perfion.APIService.Url" and we'll auto-wire it up.

					// Enable archiving lets us create an entry in the archive with the data retrieved from Perfion
					.EnableArchiving()
				));
        }
    }

	public class PerfionTask : Task
	{
		private readonly IPerfionService _perfion;

		public PerfionTask(IPerfionService perfion)
		{
			_perfion = perfion;
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			PerfionXml xml = _perfion.Query(@"
<Query>
	<Select languages='EN'>
		<Feature id='**' />
	</Select>
	<From id='Product,Category'/>
</Query>");

			Tree<PerfionXml.Component, string, int> categories = xml.Components("Category")
				.ToTree(x => x.Id, (x, p) => x.ParentId.HasValue ? p.Value(x.ParentId.Value) : p.None, x => x.Name());

			var treeVisualization = new StringBuilder();

			foreach (var level1Category in categories)
			{
				treeVisualization.AppendLine(level1Category.Model);

				foreach (var level2Category in level1Category)
				{
					treeVisualization.AppendLine(String.Format("-- {0}", level2Category.Model));
				}
			}

			context.Log.Message(treeVisualization.ToString());

			foreach (PerfionXml.Component product in xml.Components("Product"))
			{
				PerfionXml.Component parentCategory = product.FindRelation("Category");

				context.Log.Message("Product {0} in Category {1}", product.Name(), parentCategory.Name());

				PerfionXml.Image image = product.GetImage("Image");

				if (image != null)
				{
					// download RAW
					File.WriteAllBytes(Path.Combine(@"C:\tmp\perfion\", image.Name), 
						image.Download());

					// download thumb (100x100)
					File.WriteAllBytes(Path.Combine(@"c:\tmp\perfion\thumbs\", image.Name), 
						image.Download(new NameValueCollection { { "size", "100x100" }}));
				}
			}
		}

		public override string Description
		{
			get { return "TBD"; }
		}
	}
}
```

[Back to Table of Contents](#table-of-contents)

## Integrating Hangfire

To setup a Hangfire integration, start by adding the following package:

  ```
  Install-Package Vertica.Integration.Hangfire
  ```

[Back to Table of Contents](#table-of-contents)

## How to Disable IntegrationDb

It is possible to disable the IntegrationDb entirely, if you are using the Integration Service in a way where the requirement on an underlying database seems overkill. 
Maybe you're using it to expose some few HTTP services or maybe you're using it as a "Run-Once" Legacy Migration platform.

Disabling the IntegrationDb is easy. Use the **.Database(...)** method on part of bootstrapping Integration Service.

```c#
namespace ConsoleApplication16
{
    class Program
    {
        static void Main(string[] args)
        {
            IntegrationStartup.Run(args, application => application
                .Database(database => database.DisableIntegrationDb()));
        }
    }
}
```

**NOTE** If you Disable IntegrationDb, we'll try to use fallback implementations for some of the built-in functionality, e.g. logging, configuration and archiving, but if some dependency requires the IntegrationDb (IDbFactory), Integration Service will make sure to provide a detailed error message:
```
Unhandled Exception: Vertica.Integration.Infrastructure.Database.Databases.DatabaseDisabledException: IntegrationDb has been disabled.

Examine the DependencyChain below to see which component has a dependency of this:

Component 'Late bound Vertica.Integration.Infrastructure.Database.IDbFactory'1[[Vertica.Integration.Infrastructure.Database.DefaultConnection, Vertica.Integration, Version=1.4.5638.24340, Culture=neutral, PublicKeyToken=null]]' resolved as
dependency of
        component 'Late bound Vertica.Integration.Infrastructure.Database.IDbFactory'1[[Vertica.Integration.Infrastructure.Database.DefaultConnection, Vertica.Integration, Version=1.4.5638.24340, Culture=neutral, PublicKeyToken=null]]' resolved as dependency of
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

Integration Service uses Castle Windsor (https://github.com/castleproject/Windsor/blob/master/docs/README.md) as its IoC container. Everything you can do with Castle Windsor - you can do with Integration Service.

The extension point is easy, like other parts, it takes place in the Bootstrapping code of Integration Service.

The example below illustrates how to use built-in **Install**-class to auto-register classes based on conventions.

```c#
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Model;

namespace ConsoleApplication16
{
    class Program
    {
        static void Main(string[] args)
        {
            IntegrationStartup.Run(args, application => application
                .AddCustomInstaller(Install.ByConvention.AddFromAssemblyOfThis<ISomeService>())
                .Tasks(tasks => tasks.Task<UseSomeServiceTask>()));
        }
    }

    public interface ISomeService
    {
        void DoSomething();
    }

    public class SomeService : ISomeService
    {
        public void DoSomething()
        {
            // does something
        }
    }

    public class UseSomeServiceTask : Task
    {
        private readonly ISomeService _service;

        public UseSomeServiceTask(ISomeService service)
        {
            _service = service;
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            _service.DoSomething();
        }

        public override string Description
        {
            get { return "Illustrates how to use a custom service."; }
        }
    }
}
```

If you need more control or have specific requirements to how Integration Service should resolve your custom services/classes, you can provide your own Castle Windsor Installer implementation. The example below shows how to do just that:

```c#
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Model;

namespace ConsoleApplication16
{
    class Program
    {
        static void Main(string[] args)
        {
            IntegrationStartup.Run(args, application => application
                .AddCustomInstaller(new CustomInstaller())
                .Tasks(tasks => tasks
                    .Task<UseComplexFactoryTask>()));
        }
    }

    public class CustomInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IComplexFactory>()
                    .AsFactory());

            container.Register(
                Component.For<ComplexType>()
                    .UsingFactoryMethod(x => new ComplexType())
                    .LifestyleTransient());
        }
    }

    public interface IComplexFactory
    {
        ComplexType Create();
    }

    public class ComplexType
    {
    }

    public class UseComplexFactoryTask : Task
    {
        private readonly IComplexFactory _factory;

        public UseComplexFactoryTask(IComplexFactory factory)
        {
            _factory = factory;
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            ComplexType instance1 = _factory.Create();
            ComplexType instance2 = _factory.Create();

            context.Log.Message("Same instance: {0}", instance1.Equals(instance2));
        }

        public override string Description
        {
            get { return "Illustrates how to use a dependency coming from a custom installer."; }
        }
    }
}
```

[Back to Table of Contents](#table-of-contents)

## How to Setup connection to a custom database

Adding a Custom Connection to a Custom Database is very easy.

The following example will walk you through the required steps.

First you'll create an entry in your configuration file, app.config, with your connection string:

```xml
<connectionStrings>
  <add name="CustomDb" connectionString="Integrated Security=SSPI;Data Source=[NAME-OF-SQL-SERVER];Database=[NAME-OF-CUSTOM-DATABASE]" />
</connectionStrings>  
```

Next step is to create a **public class** that inherits from *Vertica.Integration.Infrastructure.Database.Connection*.

In the constructor of this class you can then reference the connection string by name:

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

**Note:** You can also use **ConnectionString.FromText("...")** if you just want to hard-code your connection string.

The next step is to register the database connection, this is done in the initial configuration:

```c#
namespace ConsoleApplication16
{
    class Program
    {
        static void Main(string[] args)
        {
            IntegrationStartup.Run(args, application => application
                .Database(database => database
                    .AddConnection(new CustomDb())));
        }
    }
}
```
You can now access the database connection, through the generic **IDbFactory<>** interface. 

The following example shows how to do that from a Task.

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
                // session.Execute("...");
				// session.Query<string>("SELECT 'HELLO';");
				// ... more on https://github.com/StackExchange/dapper-dot-net
				// ... see below for advanced example on how to use full capabilities of Dapper

                transaction.Commit();
            }
        }
    }
}
```

###IDbFactory<<TConnection>>
This factory gives you access to any custom connection that you register.

The Factory exposes the underlying IDbConnection but more importantly it allows you to create an **IDbSession** against that connection. 
The **IDbSession** is a very thin Adapter on top of Dapper (https://github.com/StackExchange/dapper-dot-net), giving you few but powerful options to work against your database. If you need to open up the full capabilities of Dapper, then you can simply use it's extension methods, see example below:

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

## How to setup MonitorFoldersStep

TBD. 
[Back to Table of Contents](#table-of-contents)

## How to Extend MonitorTask

This example shows how you can add a custom step to the existing **MonitorTask**. You can read more about **MonitorTask** [here](#built-in-tasks). 

In this complete example, a new Step "MonitorLowDiscSpaceStep" is created which will monitor low drive space:

```c#
using System;
using System.IO;
using System.Linq;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Utilities_v4;

namespace ConsoleApplication16
{
    class Program
    {
        static void Main(string[] args)
        {
            IntegrationStartup.Run(args, application => application
                .Tasks(tasks => tasks
                    .MonitorTask(task => task
                        .Step<MonitorLowDiskSpaceStep>())));
        }
    }

    public class MonitorLowDiskSpaceStep : Step<MonitorWorkItem>
    {
        public override void Execute(MonitorWorkItem workItem, ITaskExecutionContext context)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives().Where(x => x.DriveType == DriveType.Fixed))
            {
                double availablePercentage  = ((double)drive.TotalFreeSpace / drive.TotalSize) * 100;

                if (availablePercentage <= 15d)
                {
                    workItem.Add(
                        Time.Now,
                        Environment.MachineName,
                        String.Format("[WARNING] Disk {0} ({1}) is running low ({2:0.00} % free space available).",
                            drive.VolumeLabel,
                            drive.Name,
                            availablePercentage),
                        Target.Service);
                }
            }
        }

        public override string Description
        {
            get { return "Calculates available space per logical drive and warns if disk is running low (<= 15%)."; }
        }
    }
}
```

The Task is executed by running the Integration Service with the following arguments:
```.exe MonitorTask```

Other examples of customizing the **MonitorTask** could be to:

1. Monitor synchronized data for common data issues, e.g. invalid discounts, missing required data on customers and such
2. Export errors from Sitecore, uCommerce, umbraco and other platforms exposing a log

[Back to Table of Contents](#table-of-contents)

## How to setup ArchiveFoldersStep

IIS log file archiving.
Sitecore log file archiving.

TBD. 
[Back to Table of Contents](#table-of-contents)

## How to Extend MaintenanceTask

This example shows how you can add a custom step to the existing **MaintenanceTask**. You can read more about **MaintenanceTask** [here](#built-in-tasks). 

In this complete example, a new Step "UCommerceIndexMaintenanceStep" is created which will perform index maintenance using a [custom database connection](#how-to-setup-connection-to-custom-database) to uCommerce database (http://www.ucommerce.net).

```c#
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Model;

namespace ConsoleApplication16
{
    class Program
    {
        static void Main(string[] args)
        {
            IntegrationStartup.Run(args, application => application
                .Database(database => database
                    .AddConnection(new UCommerceDb(ConnectionString.FromText("Integrated Security=SSPI;Data Source=.;Database=uCommerceDemoStore"))))
                .Tasks(tasks => tasks
                    .MaintenanceTask(task => task
                        .Step<UCommerceIndexMaintenanceStep>())));
        }
    }

    public class UCommerceDb : Connection
    {
        public UCommerceDb(ConnectionString connectionString)
            : base(connectionString)
        {
        }
    }

    public class UCommerceIndexMaintenanceStep : Step<MaintenanceWorkItem>
    {
        private readonly IDbFactory<UCommerceDb> _uCommerceDb;

        public UCommerceIndexMaintenanceStep(IDbFactory<UCommerceDb> uCommerceDb)
        {
            _uCommerceDb = uCommerceDb;
        }

        public override void Execute(MaintenanceWorkItem workItem, ITaskExecutionContext context)
        {
            using (IDbSession session = _uCommerceDb.OpenSession())
            {
                session.Execute(@"
ALTER INDEX [uCommerce_PK_Order] ON [dbo].[uCommerce_PurchaseOrder] REORGANIZE
ALTER INDEX [IX_Order] ON [dbo].[uCommerce_PurchaseOrder] REORGANIZE
ALTER INDEX [IX_uCommerce_PurchaseOrder_BasketId] ON [dbo].[uCommerce_PurchaseOrder] REORGANIZE
");
            }
        }

        public override string Description
        {
            get { return "Performs index maintenance on certain uCommerce tables."; }
        }
    }
}
```

The Task is executed by running the Integration Service with the following arguments:
```.exe MaintenanceTask```

Other examples of customizing the **MaintenanceTask** could be to:

1. Restart IIS AppPool on one or more servers
2. Archive Sitecore log files
3. Archive IIS log files
4. Archive MongoDB log files

[Back to Table of Contents](#table-of-contents)