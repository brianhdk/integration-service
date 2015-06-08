# Integration Service
General purpose platform for running Tasks and Migrations expose (internally) HTTP Services and much much more!

## Get started

1. Choosing a Host for Integration Service
Typically Integration Service is hosted through a simple .NET Console Application (.exe). Add a new "Console Application" project to your existing (or new solution). 

  **NOTE:** Later you'll add a Class Library project where all your actual implementation code will be placed.

2. Install Integration Service via NuGet
  ```
  Install-Package Vertica.Integration.Host
  ```
  The package above will add all necessary files to get you up and running soon.
3. Modify "Program.cs" as mentioned in the "Readme.txt"
  ```
  namespace NameOfProject
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
  ```
  <connectionStrings>
      <add name="IntegrationDb" connectionString="Integrated Security=SSPI;Data Source=[NAME-OF-SQL-SERVER];Database=[NAME-OF-INTEGRATION-DATABASE]" />
  </connectionStrings>  
  ```

  Azure example:
  ```
  <add name="IntegrationDb" connectionString="Server=tcp:xxxx.database.windows.net,1433;Database=IntegrationDb;User ID=xxxx@xxxx;Password=xxxx;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" />
  ```  

  *By default Integration Service requires a database - but this can be disabled. Look for "Disabling database" to read more about the option of running Integration Service without a database.*
  
  ### SMTP
  ```
  <mailSettings>
      <smtp from="[EMAIL-ADDRESS]">
          <network host="[SMTP-HOST]" />
      </smtp>
  </mailSettings>
  ```
  
  Mandrill example:
  ```
  <smtp from="xxxx@yyyy.zzzz">
        <network host="smtp.mandrillapp.com" userName="xxxx" password="xxxx" port="587" />
  </smtp>
  ```    

4. Run "MigrateTask" to ensure an up-to-date-schema
 - From Visual Studio, open the Project Properties of your Console Application project, navigate to "Debug"-tab, and write "MigrateTask" (without quotes) in the "Command line arguments"-textbox.
 - Run it => (CTRL+F5 or F5)
 - If the MigrateTask fails you need to make sure that you have all necessary permissions to the database specified earlier (effectively we're changing the Db schema, and potentially creating a new database - so you need a lot of permission!)

5. Next step is to create a new Class Library project to contain your actual code/implementations. This is recommended to enforce separation, but it's not required. After creating your new Class Library project, install the following NuGet package to that project:
  ```
  Install-Package Vertica.Integration
  ```
6. You're now up and running with the Integration Service. Search the documentation to find examples on how to start using it, e.g. how to Tasks, how to setup custom Migrations, expose HTTP services, setup the Management Portal and much more. Good luck! Remember - any feedback is very much appreciated.
