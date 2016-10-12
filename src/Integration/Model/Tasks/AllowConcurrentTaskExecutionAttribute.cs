using System;

namespace Vertica.Integration.Model.Tasks
{
    /// <summary>
    /// Allows for a task to be executed in parallel.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AllowConcurrentTaskExecutionAttribute : Attribute
    {
    }
}