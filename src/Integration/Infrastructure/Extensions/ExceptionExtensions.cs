using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace Vertica.Integration.Infrastructure.Extensions
{
    public static class ExceptionExtensions
    {
        public static string GetFullStacktrace(this Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

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

        public static string AggregateMessages(this AggregateException exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            return string.Join(Environment.NewLine, exception.InnerExceptions.Select(AggregateMessages));
        }

        public static string AggregateMessages(this Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            var sb = new StringBuilder();

            int[] indents = { 0 };

            Func<string, string> makeIndent = msg => string.Concat(new string('-', indents[0] * 3), " ", msg).Trim();

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