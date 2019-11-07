using System;
using System.Linq;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Extensions;

namespace Vertica.Integration.Tests.Infrastructure
{
	[TestFixture]
    [Obsolete("Will be removed.")]
    public class RepeatTester
	{
		[Test]
		public void Repeat_X_Times()
		{
			var result = 3.Times().Select(x => x).ToArray();

			CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
		}
	}
}