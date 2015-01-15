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

        public TaskNotFoundException(string taskName)
            : base(String.Format("Task with name '{0}' not found.", taskName))
        {
        }

        protected TaskNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}