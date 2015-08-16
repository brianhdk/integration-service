using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Parsing;

namespace Vertica.Integration.Tests.Infrastructure.Parsing
{
    [TestFixture]
    public class CsvParserTester
    {
        [Test]
        public void Parse_NoHeader_Different_Delimiter()
        {
            CsvRow[] rows = Parse(@"Row1-Field1,Row1-Field2
Row2-Field1,Row2-Field2",
                csv => csv
					.NoHeaders()
					.ChangeDelimiter(","));

            Assert.That(rows.Length, Is.EqualTo(2));
            Assert.That(rows[0][0], Is.EqualTo("Row1-Field1"));
            Assert.That(rows[0][1], Is.EqualTo("Row1-Field2"));
            Assert.That(rows[1][0], Is.EqualTo("Row2-Field1"));
            Assert.That(rows[1][1], Is.EqualTo("Row2-Field2"));

            Assert.That(rows[0].ToString(), Is.EqualTo("Row1-Field1,Row1-Field2"));
            Assert.That(rows[1].ToString(), Is.EqualTo("Row2-Field1,Row2-Field2"));
        }

        [Test]
        public void Parse_Header_Default_Delimiter()
        {
            CsvRow[] rows = Parse(@"Field1;Field2
Row1-Field1;Row1-Field2
Row2-Field1;Row2-Field2");

            Assert.That(rows.Length, Is.EqualTo(2));

            Assert.That(rows[0][0], Is.EqualTo("Row1-Field1"));
            Assert.That(rows[0]["Field1"], Is.EqualTo("Row1-Field1"));
            Assert.That(((dynamic)rows[0]).Field1, Is.EqualTo("Row1-Field1"));

            Assert.That(rows[0][1], Is.EqualTo("Row1-Field2"));
            Assert.That(rows[0]["Field2"], Is.EqualTo("Row1-Field2"));
            Assert.That(((dynamic)rows[0]).Field2, Is.EqualTo("Row1-Field2"));

            Assert.That(rows[1][0], Is.EqualTo("Row2-Field1"));
            Assert.That(rows[1]["Field1"], Is.EqualTo("Row2-Field1"));
            Assert.That(((dynamic)rows[1]).Field1, Is.EqualTo("Row2-Field1"));

            Assert.That(rows[1][1], Is.EqualTo("Row2-Field2"));
            Assert.That(rows[1]["Field2"], Is.EqualTo("Row2-Field2"));
            Assert.That(((dynamic)rows[1]).Field2, Is.EqualTo("Row2-Field2"));

            Assert.That(rows[0].ToString(), Is.EqualTo("Row1-Field1;Row1-Field2"));
            Assert.That(rows[1].ToString(), Is.EqualTo("Row2-Field1;Row2-Field2"));
        }

        [Test]
        public void Parse_NoHeader_Returns_Delimiter_LinesNumbers_But_No_Headers()
        {
            CsvRow[] rows = Parse(@"Row1-Field1,Row1-Field2
Row2-Field1,Row2-Field2", csv => csv.NoHeaders());

            Assert.That(rows[0].Meta.LineNumber, Is.EqualTo(1));
            Assert.That(rows[0].Meta.Delimiter, Is.EqualTo(CsvConfiguration.DefaultDelimiter));
            Assert.That(rows[0].Meta.Headers, Is.EqualTo(null));
            Assert.That(rows[1].Meta.LineNumber, Is.EqualTo(2));
            Assert.That(rows[1].Meta.Delimiter, Is.EqualTo(CsvConfiguration.DefaultDelimiter));
            Assert.That(rows[1].Meta.Headers, Is.EqualTo(null));
        }

        [Test]
        public void Parse_WithHeader_Returns_Delimiter_LinesNumbers_But_No_Headers()
        {
            CsvRow[] rows = Parse(@"Field1;Field2
Row1-Field1;Row1-Field2
Row2-Field1;Row2-Field2");

            Assert.That(rows[0].Meta.LineNumber, Is.EqualTo(2));
            Assert.That(rows[0].Meta.Delimiter, Is.EqualTo(CsvConfiguration.DefaultDelimiter));
            CollectionAssert.AreEqual(new[] { "Field1", "Field2" }, rows[0].Meta.Headers);
            Assert.That(rows[1].Meta.LineNumber, Is.EqualTo(3));
            Assert.That(rows[1].Meta.Delimiter, Is.EqualTo(CsvConfiguration.DefaultDelimiter));
            CollectionAssert.AreEqual(new[] { "Field1", "Field2" }, rows[1].Meta.Headers);
        }

        private CsvRow[] Parse(string data, Action<CsvConfiguration> csv = null)
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(data);
                writer.Flush();
                stream.Position = 0;

                var subject = new CsvParser();

                return subject.Parse(stream, csv).ToArray();
            }
        }
    }
}