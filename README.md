# Integration Service
General purpose platform for running Tasks and Migrations expose (internally) HTTP Services and much much more!

## Table of Contents
 - [How to Get Started](#how-to-get-started)
 - [Basics of Tasks](#basics-of-tasks)
 - [Bootstrapping Tasks](#bootstrapping-tasks)
 - [Built-in Tasks](#built-in-tasks) 
 - [Task Execution Flow](#task-execution-flow)  
 - [Basics of WebApi](#basics-of-webapi)
 - [Logging and Exceptions](#logging-and-exceptions) 
 - [Command Line Reference](#command-line-reference)
 - [Migrations](#migrations)
 - [Built-in Services](#built-in-services)
 - [Configurations](#configurations)
 - [Archives](#archives)
 - [CSV](#csv)
 - [FTP](#ftp)
 - [Monitoring](#monitoring)
 - [Maintenance](#maintenance)
 - [Setting up Portal](#setting-up-portal)
 - [Integrating Elmah](#integrating-elmah)
 - [Integrating Azure - BlobStorage](#integrating-azure---blobstorage)
 - [How to Disable IntegrationDb](#how-to-disable-integrationdb)
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

<<ADD>>
<<REMOVE>>
<<CLEAR>>
<<TASKS WITH STEPS>>
<<EXTENSION METHODS>>

[Back to Table of Contents](#table-of-contents)

## Built-in Tasks

1. MonitorTask
  * Has built-in steps
	* ExportIntegrationErrorsStep - exports errors and warnings from the IntegrationDb itself
	* PingUrlsStep - performs an http(s) request to a predefined set of URL's
  * Can easily be extended with additional Steps to Monitor other parts of the solution
	* See [How to Extend MonitorTask](#how-to-extend-monitortask)
  * Sends out an e-mail with the error/warning messages
  * Use **MonitorConfiguration** for configuration
	* Also see [Migration of MonitorConfiguration](#migration-of-monitorconfiguration)
2. MaintenanceTask
  * Performs a number of clean-up related tasks, including...
3. WriteDocumentationTask
  * Simple Task that uses the **ITaskFactory** to iterate all registred Tasks. This can produce a simple TXT file with all tasks and steps mentioned.
4. MigrateTask
  * Runs migrations of own ... custom...

[Back to Table of Contents](#table-of-contents)

## Task Execution Flow

<<LOGGING, CONTINUE WITH/BREAK - VISUAL EXAMPLE>>

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

### Migration of MonitorConfiguration

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

## Monitoring

TBD. 
[Back to Table of Contents](#table-of-contents)

## Maintenance

TBD. 
[Back to Table of Contents](#table-of-contents)

## Setting up Portal

TBD. 
[Back to Table of Contents](#table-of-contents)

## Integrating Elmah

TBD. 
[Back to Table of Contents](#table-of-contents)

## Integrating Azure - BlobStorage

TBD. 
[Back to Table of Contents](#table-of-contents)

## How to Disable IntegrationDb

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
