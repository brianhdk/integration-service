using System;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Extensions
{
    public static class NameExtensions
    {
        public static string Name(this ITask task)
        {
            if (task == null) throw new ArgumentNullException("task");

            return task.GetType().Name;
        }

        public static string Name(this IStep step)
        {
            if (step == null) throw new ArgumentNullException("step");

            return step.GetType().Name;
        }
    }
}