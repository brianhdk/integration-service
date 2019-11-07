using System;
using System.Runtime.Serialization;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.Model.Exceptions
{
    [Serializable]
    public class NoHostFoundException : Exception
    {
        public NoHostFoundException()
        {
        }

	    internal NoHostFoundException(HostArguments args)
            : base(
	            $@"None of the configured {typeof (IHost).FullName} are able to handle arguments: 
{args}

If you are trying to execute a Task, make sure that you are spelling the name of the Task correctly.

Consider running the ""WriteDocumentationTask"" to get a text-output of available tasks and hosts.

To configure Hosts in general, use the .Hosts(hosts => hosts.Host<YourHost>) method part of the initial configuration."
	            )
        {
        }

		protected NoHostFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}