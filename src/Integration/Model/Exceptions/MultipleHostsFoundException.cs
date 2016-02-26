using System;
using System.Linq;
using System.Runtime.Serialization;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.Model.Exceptions
{
    [Serializable]
    public class MultipleHostsFoundException : Exception
    {
        public MultipleHostsFoundException()
        {
        }

	    internal MultipleHostsFoundException(HostArguments args, IHost[] hosts)
            : base(
	            $@"Multiple hosts was found to handle arguments: 
{args}

The hosts are:

{string.Join(", ",
		            hosts.Select(x => x.Name()))}

To fix this problem you need to inspect the ""CanHandle""-method of the hosts to find out why the criteria is met."
	            )
        {
        }

		protected MultipleHostsFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}