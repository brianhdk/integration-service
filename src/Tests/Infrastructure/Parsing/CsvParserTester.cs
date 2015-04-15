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
            CsvRow[] rows = Parse(@"Row1-Field1;Row1-Field2
Row2-Field1;Row2-Field2",
                false,
                builder => builder.ChangeDelimiter(";"));

            Assert.That(rows.Length, Is.EqualTo(2));
            Assert.That(rows[0][0], Is.EqualTo("Row1-Field1"));
            Assert.That(rows[0][1], Is.EqualTo("Row1-Field2"));
            Assert.That(rows[1][0], Is.EqualTo("Row2-Field1"));
            Assert.That(rows[1][1], Is.EqualTo("Row2-Field2"));

            Assert.That(rows[0].ToString(), Is.EqualTo("Row1-Field1;Row1-Field2"));
            Assert.That(rows[1].ToString(), Is.EqualTo("Row2-Field1;Row2-Field2"));
        }

        [Test]
        public void Parse_Header_Default_Delimiter()
        {
            CsvRow[] rows = Parse(@"Field1,Field2
Row1-Field1,Row1-Field2
Row2-Field1,Row2-Field2",
                true);

            Assert.That(rows.Length, Is.EqualTo(2));

            Assert.That(rows[0][0], Is.EqualTo("Row1-Field1"));
            Assert.That(rows[0]["Field1"], Is.EqualTo("Row1-Field1"));
            Assert.That(((dynamic) rows[0]).Field1, Is.EqualTo("Row1-Field1"));

            Assert.That(rows[0][1], Is.EqualTo("Row1-Field2"));
            Assert.That(rows[0]["Field2"], Is.EqualTo("Row1-Field2"));
            Assert.That(((dynamic)rows[0]).Field2, Is.EqualTo("Row1-Field2"));

            Assert.That(rows[1][0], Is.EqualTo("Row2-Field1"));
            Assert.That(rows[1]["Field1"], Is.EqualTo("Row2-Field1"));
            Assert.That(((dynamic)rows[1]).Field1, Is.EqualTo("Row2-Field1"));

            Assert.That(rows[1][1], Is.EqualTo("Row2-Field2"));
            Assert.That(rows[1]["Field2"], Is.EqualTo("Row2-Field2"));
            Assert.That(((dynamic)rows[1]).Field2, Is.EqualTo("Row2-Field2"));

            Assert.That(rows[0].ToString(), Is.EqualTo("Row1-Field1,Row1-Field2"));
            Assert.That(rows[1].ToString(), Is.EqualTo("Row2-Field1,Row2-Field2"));
        }

        private CsvRow[] Parse(string data, bool firstLineIsHeader, Action<CsvConfiguration> builder = null)
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(data);
                writer.Flush();
                stream.Position = 0;

                var subject = new CsvParser(new CsvReader());

                return subject.Parse(stream, firstLineIsHeader, builder).ToArray();
            }
        }
    }
}