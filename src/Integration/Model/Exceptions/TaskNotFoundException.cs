using System;
using System.Runtime.Serialization;

namespace Vertica.Integration.Model.Exceptions
{
    [Serializable]
    public class TaskNotFoundException : Exception
    {
        public TaskNotFoundException()
        {
        }

	    internal TaskNotFoundException(string taskName)
            : base(
	            $@"Task with name '{taskName}' not found. 
Consider running the ""WriteDocumentationTask"" to get a text-output of available tasks.
If your task is not listed then you probably forgot to register it. Use Tasks(tasks => tasks.Task<YourTask>)"
	            )
        {
        }

        protected TaskNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}