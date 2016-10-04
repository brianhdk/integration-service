using System;

namespace Vertica.Integration.Model.Tasks
{
    /// <summary>
    /// Allows for a task to be executed
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AllowConcurrentExecutionAttribute : Attribute
    {
    }
}