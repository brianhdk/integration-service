using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.Tests.Model.Hosting
{
    [TestFixture]
    public class ArgumentsParserTester
    {
        [TestCase("Task", "Task", null, null, null, null)]
        [TestCase("Task -service:install Args", "Task", "service", "install", "Args", null)]
        [TestCase("Task -service:install", "Task", "service", "install", null, null)]
        [TestCase("Task -service", "Task", "service", null, null, null)]
        [TestCase("Task -service Args", "Task", "service", null, "Args", null)]
        [TestCase("Task -service: install Args: c:\\", "Task", "service", "install", "Args", "c:\\")]
        [TestCase("Task -service : install Args : Value", "Task", "service", "install", "Args", "Value")]
        [TestCase("Task -service :install Args:Value", "Task", "service", "install", "Args", "Value")]
        public void Parse_TestCases(string arguments, string expectedCommand, string expectedSingleCommandArgsKey, string expectedSingleCommandArgsValue, string expectedSingleArgsKey, string expectedSingleArgsValue)
        {
            var subject = new ArgumentsParser();
            HostArguments result = subject.Parse(ToArguments(arguments));

            Assert.That(result.Command, Is.EqualTo(expectedCommand));

            KeyValuePair<string, string> commandArgs = result.CommandArgs.SingleOrDefault();
            Assert.That(commandArgs.Key, Is.EqualTo(expectedSingleCommandArgsKey));
            Assert.That(commandArgs.Value, Is.EqualTo(expectedSingleCommandArgsValue));

            KeyValuePair<string, string> args = result.Args.SingleOrDefault();
            Assert.That(args.Key, Is.EqualTo(expectedSingleArgsKey));
            Assert.That(args.Value, Is.EqualTo(expectedSingleArgsValue));
        }

        [Test]
        public void Parse_Complex_Arguments()
        {
            var subject = new ArgumentsParser();
            HostArguments result = subject.Parse(ToArguments("WriteDocumentationTask -service:install -url:http://localhost:8123 ToFile: c:\\test.csv Format:CSV Append : False Local"));

            Assert.That(result.Command, Is.EqualTo("WriteDocumentationTask"));

            Assert.IsTrue(result.CommandArgs.Contains("service"), "result.CommandArgs.ContainsKey('service')");
            Assert.That(result.CommandArgs["service"], Is.EqualTo("install"));
            Assert.IsTrue(result.CommandArgs.Contains("url"), "result.CommandArgs.ContainsKey('url')");
            Assert.That(result.CommandArgs["url"], Is.EqualTo("http://localhost:8123"));

            Assert.IsTrue(result.Args.Contains("ToFile"), "result.CommandArgs.ContainsKey('ToFile')");
            Assert.That(result.Args["tofile"], Is.EqualTo("c:\\test.csv"));
            Assert.IsTrue(result.Args.Contains("format"), "result.CommandArgs.ContainsKey('format')");
            Assert.That(result.Args["Format"], Is.EqualTo("CSV"));
            Assert.IsTrue(result.Args.Contains("Append"), "result.CommandArgs.ContainsKey('Append')");
            Assert.That(result.Args["Append"], Is.EqualTo("False"));
            Assert.IsTrue(result.Args.Contains("Local"), "result.CommandArgs.ContainsKey('Local')");
            Assert.That(result.Args["Local"], Is.Null);
        }

        private static string[] ToArguments(string args)
        {
            return args.Split(new [] { " " }, StringSplitOptions.RemoveEmptyEntries);
        }
    }



    // WriteDocumentationTask -service:install -url:http://localhost:8123 [arguments]    => Install Service
    // WriteDocumentationTask -service:install -repeat:10(parse-as-timespan) [arguments]                    => Install Service
    // WriteDocuemntationTask -service:uninstall                                         => Uninstall Service

    // WebApi -service:install -url:http://localhost:8123                                     => Install Service
    // WebApi -service:uninstall                                                         => Uninstall Service

    // Rebus -service:install
    // Rebus -service:uninstall
    // WriteDocumentationTask [arguments]                                           => Run in Console
    // WriteDocumentationTask -url:http://localhost:8123 [arguments]                => Run in Console, WebApiHost
    // WriteDocumentationTask -service:http://localhost:8123 [arguments]            => Run as Service, WebApiHost
    // WriteDocumentationTask -service:10 [arguments]                               => Run as Service, Delay
    // WebApi -url:http://localhost:8123                                                 => Run in Console
    // WebApi -service -url:http://localhost:8123                                        => Run as Service
}