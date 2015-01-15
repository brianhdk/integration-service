using System;
using System.Collections;
using System.Text;

namespace Vertica.Integration.Infrastructure.Extensions
{
    public static class ExceptionExtensions
    {
        public static string GetFullStacktrace(this Exception exception)
        {
            var sb = new StringBuilder();

            while (exception != null)
            {
                sb.AppendLine(exception.GetType().FullName);
                sb.AppendLine(exception.Message);
                sb.AppendLine();
                sb.Append(exception.StackTrace);

	            foreach (DictionaryEntry entry in exception.Data)
	            {
		            sb.AppendFormat("{0} = {1}", entry.Key, entry.Value);
		            sb.AppendLine();
	            }

                exception = exception.InnerException;

                if (exception != null)
                    sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}