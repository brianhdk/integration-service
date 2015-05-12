using System;
using System.Collections;
using System.Text;

namespace Vertica.Integration.Infrastructure.Extensions
{
    internal static class ExceptionExtensions
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

        public static string AggregateMessages(this Exception exception)
        {
            var sb = new StringBuilder();

            int[] indents = { 0 };

            Func<string, string> makeIndent = msg => String.Concat(new string('-', indents[0] * 3), " ", msg).Trim();

            while (exception != null)
            {
                sb.AppendLine(makeIndent(exception.GetType().FullName));
                sb.Append(makeIndent(exception.Message));

                exception = exception.InnerException;

                if (exception != null)
                    sb.AppendLine();

                indents[0]++;
            }

            return sb.ToString();            
        }
    }
}