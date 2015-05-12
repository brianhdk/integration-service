using System;
using System.Runtime.Serialization;

namespace Vertica.Integration.Model.Exceptions
{
    [Serializable]
    internal class TaskNotFoundException : Exception
    {
        public TaskNotFoundException()
        {
        }

        public TaskNotFoundException(string taskName)
            : base(String.Format(@"Task with name '{0}' not found. Consider running the ""WriteDocumentationTask"" to get a text-output of available tasks.", taskName))
        {
        }

        protected TaskNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}