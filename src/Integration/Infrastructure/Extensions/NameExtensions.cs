using System;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.Infrastructure.Extensions
{
	public static class NameExtensions
    {
        public static string HostName(this Type host)
        {
            if (host == null) throw new ArgumentNullException("host");

            return host.Name;
        }

        public static string Name(this IHost host)
        {
            if (host == null) throw new ArgumentNullException("host");

            return host.GetType().HostName();
        }

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