using System;
using System.Collections.Generic;

namespace Vertica.Integration.Infrastructure.Email
{
	public abstract class EmailTemplate
	{
		public abstract string Subject { get; }
		public abstract bool IsHtml { get; }

		protected abstract string Template { get; }

		protected abstract IEnumerable<KeyValuePair<string, object>> Parameters { get; }

		internal string GetBody()
		{
		    return "TODO: Implement Razor..." + Template;
		    //var engine = new VelocityEngine();
		    //engine.Init(new ExtendedProperties());

		    //var context = new VelocityContext();
		    //context.Put("helper", new Helper());

		    //foreach (var parameter in Parameters ?? new KeyValuePair<string, object>[0])
		    //    context.Put(parameter.Key, parameter.Value);

		    //using (var writer = new StringWriter())
		    //{
		    //    engine.Evaluate(context, writer, String.Empty, Template);
		    //    writer.Flush();

		    //    return writer.GetStringBuilder().ToString();
		    //}
		}

		public class Helper
		{
			public string LinesToHtml(string value)
			{
				return (value ?? String.Empty).Replace(Environment.NewLine, "<br />");
			}
		}
	}
}