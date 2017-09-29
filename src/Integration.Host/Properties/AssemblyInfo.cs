using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Vertica.Integration.Host")]
[assembly: AssemblyDescription("Package that installs necessary files to run the Integration platform.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Vertica A/S")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyCopyright("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("2e7497ec-ffa4-4fd2-87f6-d2750faf0682")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("2.0.0")]
[assembly: AssemblyInformationalVersion("2.0.0-alpha1")]
[assembly: AssemblyFileVersion("2.0.0.0")]

namespace Vertica.Integration.Host.Properties
{
    internal sealed class EnsureDependencyToIntegrationProject : Infrastructure.Configuration.Configuration
    {
    }
}