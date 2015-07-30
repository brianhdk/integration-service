using NUnit.Framework;
using Vertica.Integration.Infrastructure.Templating;

namespace Vertica.Integration.Tests.Infrastructure.RazorEngine
{
	[TestFixture]
	public class RazorEngineTester
	{
		[Test]
		public void WriteAttribute()
		{
			var result = InMemoryRazorEngine.Execute(@"<a class=""@String.Format(""test {0}"", 0)""></a>");
			Assert.AreEqual(@"<a class=""test 0""></a>", result);
		}

		[Test]
		public void UseFunctions()
		{
			var result = InMemoryRazorEngine.Execute(
				@"@functions {
    
    public static string Join(string[] items)
    {
       return string.Join("","", items);
    }
}
<div>@Join(new[] { ""Blue"", ""Red"", ""Green"" })</div>");

			Assert.AreEqual(@"<div>Blue,Red,Green</div>", result);
		}
		
		[Test]
		public void UseHtmlRaw()
		{
			var result = InMemoryRazorEngine.Execute(@"@Html.Raw(""<div></div>"")");
			Assert.AreEqual(@"<div></div>", result);
		}
	}
}