using System;

namespace Vertica.Integration.Model.Tasks
{
    /*
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class PreventConcurrentExecutionAttribute : Attribute
    {
        // TODO: Sikre at man via global configuration (TasksConfiguration) kan slå det til for alle.
        //  - stadig med mulighed for at AllowConcurrentExecution på en specifik task via attribut
        //  - undersøg hvordan det virker i et udviklingsmiljø
        //      - på Duba er det slået fra i UDV! - når man kører DEBUG
        //  - god ide at skrive maskinnavn på
        //  - hvis man ikke kan acquire lock, så skal den SELECT current lock (for at læse detaljer omkring denne)
    }

    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class AllowConcurrentExecutionAttribute : Attribute
    {
    }
    */
}