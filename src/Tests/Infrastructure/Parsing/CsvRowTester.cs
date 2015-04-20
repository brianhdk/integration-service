using System;
using System.Linq;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Parsing;

namespace Vertica.Integration.Tests.Infrastructure.Parsing
{
    [TestFixture]
    public class CsvRowTester
    {
        [Test]
        public void Create_Csv_No_Headers()
        {
            CsvRow[] rows = CsvRow.BeginRows()
                    .Add("John Doe", "30")
                    .Add("Jane Doe", "31")
                    .ToRows();

            var csv = String.Join(Environment.NewLine, rows.Select(x => x.ToString()));

            Assert.That(csv, Is.EqualTo(@"John Doe,30
Jane Doe,31"));
        }

        [Test]
        public void Create_Csv_With_Headers()
        {
            CsvRow[] rows = CsvRow.BeginRows("Name", "Age")
                    .Add("John Doe", "30")
                    .Add("Jane Doe", "31")
                    .ToRows();

            var csv = String.Join(Environment.NewLine,
                new[] {rows[0].Meta.Headers.ToString()}.Concat(rows.Select(x => x.ToString())));

            Assert.That(csv, Is.EqualTo(@"Name,Age
John Doe,30
Jane Doe,31"));
        }

        [Test]
        public void Create_Csv_From_List()
        {
            CsvRow[] rows = CsvRow.BeginRows("Name")
                .From(new[] {"John", "Jane"}, x => new[] {x})
                .ToRows();

            var csv = String.Join(Environment.NewLine, rows.Select(x => x.ToString()));

            Assert.That(csv, Is.EqualTo(@"John
Jane"));
        }
    }
}