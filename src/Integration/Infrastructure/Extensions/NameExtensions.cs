using System;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Extensions
{
    public static class NameExtensions
    {
        public static string TaskName(this Type task)
        {
            if (task == null) throw new ArgumentNullException("task");

            return task.Name;
        }

        public static string Name(this ITask task)
        {
            if (task == null) throw new ArgumentNullException("task");

            return task.GetType().TaskName();
        }

        public static string StepName(this Type step)
        {
            if (step == null) throw new ArgumentNullException("step");

            return step.Name;
        }

        public static string Name(this IStep step)
        {
            if (step == null) throw new ArgumentNullException("step");

            return step.GetType().StepName();
        }
    }
}