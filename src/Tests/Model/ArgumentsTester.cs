using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Vertica.Integration.Model;

namespace Vertica.Integration.Tests.Model
{
    [TestFixture]
    public class ArgumentsTester
    {
        [Test]
        public void Enumerator_Empty_NoElements()
        {
            var subject = new Arguments();

            List<KeyValuePair<string, string>> elements = subject.ToList();

            Assert.That(elements.Count, Is.EqualTo(0));
        }

        [Test]
        public void Enumerator_WithElementsKeyOnly_VerifyElements()
        {
            var subject = new Arguments("Arg1", "Arg2");

            CollectionAssert.AreEqual(subject.Select(x => x.Key), new[] {"Arg1", "Arg2"});
            CollectionAssert.AreEqual(subject.Select(x => x.Value), new[] { "Arg1", "Arg2" });
        }

        [Test]
        public void Enumerator_WithElementsKeyAndValue_VerifyElements()
        {
            var subject = new Arguments(
                new KeyValuePair<string, string>("Arg1Key", "Arg1Value"),
                new KeyValuePair<string, string>("Arg2Key", "Arg2Value"));

            CollectionAssert.AreEqual(subject.Select(x => x.Key), new[] { "Arg1Key", "Arg2Key" });
            CollectionAssert.AreEqual(subject.Select(x => x.Value), new[] { "Arg1Value", "Arg2Value" });
        }

        [Test]
        public void ToString_NoElements_Empty()
        {
            var subject = Arguments.Empty;

            Assert.IsEmpty(subject.ToString());
        }

        [Test]
        public void ToString_WithElementsKeyOnly_KeysOnly()
        {
            var subject = new Arguments("Arg1", "Arg2");

            Assert.That(subject.ToString(), Is.EqualTo("Arg1 Arg2"));
        }

        [Test]
        public void ToString_WithElementsKeyAndValue_KeyWithValue()
        {
            var subject = new Arguments(
                new KeyValuePair<string, string>("Arg1Key", "Arg1Value"),
                new KeyValuePair<string, string>("Arg2Key", "Arg2Value"));

            Assert.That(subject.ToString(), Is.EqualTo("Arg1Key:Arg1Value Arg2Key:Arg2Value"));
        }
    }
}