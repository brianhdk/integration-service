using System;
using System.Runtime.Serialization;
using Vertica.Integration.Infrastructure.Extensions;

namespace Vertica.Integration.Model.Exceptions
{
    [Serializable]
    public class TaskWithSameNameAlreadyRegistredException : Exception
    {
        public TaskWithSameNameAlreadyRegistredException()
        {
        }

	    internal TaskWithSameNameAlreadyRegistredException(Type task)
            : base(BuildMessage(task))
        {
        }

	    internal TaskWithSameNameAlreadyRegistredException(Type task, Exception innerException)
            : base(BuildMessage(task), innerException)
        {
        }

        protected TaskWithSameNameAlreadyRegistredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private static string BuildMessage(Type task)
        {
	        if (task == null) throw new ArgumentNullException("task");

	        return String.Format(@"Unable to register Task '{0}'. A task with the same name '{1}' is already registred. 
Consider running the ""WriteDocumentationTask"" to get a text-output of available tasks.", task.FullName, task.TaskName());
        }
    }
}