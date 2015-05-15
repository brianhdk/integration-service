using System;
using System.Collections.Generic;
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
            CsvRow[] rows = CsvRow
                .BeginRows()
                .Add("John Doe", "30")
                .Add("Jane Doe", "31")
                .ToRows();

            var csv = String.Join(Environment.NewLine, rows.Select(x => x.ToString()));

            Assert.That(csv, Is.EqualTo(@"John Doe,30
Jane Doe,31"));
        }

        [Test]
        public void Create_Csv_Mismatch_Headers_Data_Throws()
        {
            CsvRow.ICsvRowBuilderFinisher builder = CsvRow
                .BeginRows("Name", "Age")
                .Add("John Doe", "30");

            ArgumentException exception = 
                Assert.Throws<ArgumentException>(() => builder.Add("Jane Doe", "31", "New column"));

            Assert.That(exception.Message, Is.StringContaining("Row #2"));
            Assert.That(exception.Message, Is.StringContaining("has 3 columns"));
            Assert.That(exception.Message, Is.StringContaining("expected 2 columns"));
        }

        [Test]
        public void Create_Csv_With_Headers()
        {
            CsvRow[] rows = CsvRow
                .BeginRows("Name", "Age")
                .Configure(x => x.ReturnHeaderAsRow())
                .Add("John Doe", "30")
                .Add("Jane Doe", "31")
                .ToRows();

            Assert.That(rows.Length, Is.EqualTo(3));
            Assert.That(rows[0].Meta.LineNumber, Is.EqualTo(1));
            Assert.That(rows[1].Meta.LineNumber, Is.EqualTo(2));
            Assert.That(rows[2].Meta.LineNumber, Is.EqualTo(3));

            var csv = String.Join(Environment.NewLine, rows.Select(x => x.ToString()));

            Assert.That(csv, Is.EqualTo(@"Name,Age
John Doe,30
Jane Doe,31"));
        }

        [Test]
        public void Create_Csv_ToString()
        {
            string csv = CsvRow
                .BeginRows("Name", "Age")
                .Configure(x => x.ReturnHeaderAsRow())
                .Add("John Doe", "30")
                .Add("Jane Doe", "31")
                .ToString();

            Assert.That(csv, Is.EqualTo(@"Name,Age
John Doe,30
Jane Doe,31"));
        }

        [Test]
        public void Create_Csv_Null_Value_ToString()
        {
            string csv = CsvRow
                .BeginRows("Name", "Age")
                .Add("John Doe", "30")
                .Add("Jane Doe", null)
                .ToString();

            Assert.That(csv, Is.EqualTo(@"John Doe,30
Jane Doe,"));
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

        [Test]
        public void Create_Csv_Add_Using_Mapper()
        {
            string csv = CsvRow
                .BeginRows("Name", "Age")
                .AddUsingMapper(x => x.Map("Age", "30").Map("Name", "John Doe"))
                .AddUsingMapper(x => x.Map("Age", "31").Map("Name", "Jane Doe"))
                .ToString();

            Assert.That(csv, Is.EqualTo(@"John Doe,30
Jane Doe,31"));
        }

        [Test]
        public void Create_Csv_From_List_Using_Mapper()
        {
            CsvRow[] rows = CsvRow.BeginRows("Name")
                .FromUsingMapper(new[] { "John", "Jane" }, (m, x) => m.Map("Name", x))
                .ToRows();

            var csv = String.Join(Environment.NewLine, rows.Select(x => x.ToString()));

            Assert.That(csv, Is.EqualTo(@"John
Jane"));
        }

        [Test]
        public void Create_Csv_Add_Using_Mapper_Wrong_Header_Throws()
        {
            CsvRow.ICsvRowBuilder csv = CsvRow.BeginRows("Name");

            KeyNotFoundException exception = 
                Assert.Throws<KeyNotFoundException>(() => csv.AddUsingMapper(x => x.Map("Age", "30")));

            Assert.That(exception.Message, Is.StringContaining("Age"));
        }

        [Test]
        public void Create_Complex_Build()
        {
            CsvRow.ICsvRowBuilderAdder builder = CsvRow.BeginRows("Name", "Age")
                .Configure(x => x
                    .ReturnHeaderAsRow()
                    .ChangeDelimiter(";"));

            builder.AddUsingMapper(x => x
                .Map("Name", "John Doe")
                .Map("Age", "30"));

            builder.FromUsingMapper(new[] {"Jane Doe"}, (m, x) => m.Map("Name", x));

            string csv = builder.ToString();

            Assert.That(csv, Is.EqualTo(@"Name;Age
John Doe;30
Jane Doe;"));
        }
    }
}