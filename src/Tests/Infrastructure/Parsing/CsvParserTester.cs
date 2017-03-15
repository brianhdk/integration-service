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
            CsvRow[] rows = Parse($"Row1-Field1,Row1-Field2{Environment.NewLine}Row2-Field1,Row2-Field2",
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
        public void Parse_Tab_Delimiter()
        {
            CsvRow[] rows = Parse($"Row1-Field1\tRow1-Field2{Environment.NewLine}Row2-LongerField1\tRow2-Field2",
                csv => csv
                    .NoHeaders()
                    .ChangeDelimiter("\t"));

            Assert.That(rows.Length, Is.EqualTo(2));
            Assert.That(rows[0][0], Is.EqualTo("Row1-Field1"));
            Assert.That(rows[0][1], Is.EqualTo("Row1-Field2"));
            Assert.That(rows[1][0], Is.EqualTo("Row2-LongerField1"));
            Assert.That(rows[1][1], Is.EqualTo("Row2-Field2"));

            Assert.That(rows[0].ToString(), Is.EqualTo("Row1-Field1\tRow1-Field2"));
            Assert.That(rows[1].ToString(), Is.EqualTo("Row2-LongerField1\tRow2-Field2"));
        }

        [Test]
        public void Parse_Header_Default_Delimiter()
        {
            CsvRow[] rows = Parse(string.Join(Environment.NewLine, 
"Field1;Field2",
"Row1-Field1;Row1-Field2",
"Row2-Field1;Row2-Field2"));

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

	    [Test]
	    public void Parse_MultiLine_BuildBy_CsvRow()
	    {
		    string csv = CsvRow.BeginRows("Id", "Text")
				.Configure(configure => configure.ReturnHeaderAsRow())
			    .AddUsingMapper(mapper => mapper.Map("Id", "1").Map("Text", "SingleLine-1"))
			    .AddUsingMapper(mapper => mapper.Map("Id", "2").Map("Text", $"Multi{Environment.NewLine}Line-2"))
			    .AddUsingMapper(mapper => mapper.Map("Id", "3").Map("Text", "SingleLine-3"))
			    .ToString();

		    CsvRow[] rows = Parse(csv);

		    Assert.That(rows.Length, Is.EqualTo(3));
		    Assert.That(rows[0]["Id"], Is.EqualTo("1"));
			Assert.That(rows[1]["Id"], Is.EqualTo("2"));
			Assert.That(rows[2]["Id"], Is.EqualTo("3"));

			Assert.That(rows[0]["Text"], Is.EqualTo("SingleLine-1"));
			Assert.That(rows[1]["Text"], Is.EqualTo($"Multi{Environment.NewLine}Line-2"));
			Assert.That(rows[2]["Text"], Is.EqualTo("SingleLine-3"));
	    }

		[Test]
		public void Parse_MultiLine_NoQuotations()
		{
		    string csv = string.Join(Environment.NewLine,
		        "PartNumber|LanguageId|Name|ShortDescription|LongDescription|AuxDescription1|AuxDescription2|Keyword|Published|Delete",
		        "030445|-1|Longines Ladies' Présence Watch|Quartz. Sapphire crystal. Gold-plated. Black leather strap. White dial. Date. Water-|Quartz. Sapphire crystal. Gold-plated. Black leather strap. White dial. Date. Water-resistant 30 m. 23 mm case.||||1|0",
		        "030459|-1|Viktor & Rolf Flowerbomb EdP|The ultimate concentrate for enhancing skin's inherent multi-defensive power against|The ultimate concentrate for enhancing skin's inherent multi-defensive power against signs of aging, environmental factors and daily stress. Day by day, your skin becomes smoother and more resilient making wrinkles less noticeable and your complexion appears to glow with more radiance than ever. Ultimune Power Infusing Concentrate is for all women of all ages. It works with your skincare to boost the benefits of your regimen, no matter your concerns.",
		        "Benefits: Shiseido's exclusive Ultimune Complex helps boost defensive functions that have declined in Langerhans Cells, the cells that hold the key to promoting skin's multi-defensive power. Now your skin can achieve its greatest beauty potential. Immediately, skin feels full and supple, with a silky-smooth surface. In one week, skin appears to glow more than ever. In 4 weeks: firmness and resilience are improved, making wrinkles less visible. Apply morning and night after cleansing and softening the face. When using other serums, apply Ultimune first to enhance the benefits of the following treatments.||||1|0",
		        "030460|-1|Viktor & Rolf Flowerbomb EdT|A new chapter in the Flowerbomb story. A true bouquet of flowers that are good enoug|A new chapter in the Flowerbomb story. A true bouquet of flowers that are good enough to eat! With crispy buds that evoke the “morning dew”. This Fragrance is multi-faceted like the bottle it comes in: a mille-feuille of Flowerbomb flowers around freesia, centifolia rose and Sambac jasmine, refreshed by crispy green notes and a touch of mandarin and bergamot.||||1|0");

			CsvRow[] rows = Parse(csv, configure => configure.ChangeDelimiter("|"));

			Assert.That(rows.Length, Is.EqualTo(3));
			Assert.That(rows[0]["PartNumber"], Is.EqualTo("030445"));
			Assert.That(rows[1]["PartNumber"], Is.EqualTo("030459"));
			Assert.That(rows[2]["PartNumber"], Is.EqualTo("030460"));

			Assert.That(rows[0]["LongDescription"], Is.EqualTo("Quartz. Sapphire crystal. Gold-plated. Black leather strap. White dial. Date. Water-resistant 30 m. 23 mm case."));
			Assert.That(rows[1]["LongDescription"], Is.EqualTo($"The ultimate concentrate for enhancing skin's inherent multi-defensive power against signs of aging, environmental factors and daily stress. Day by day, your skin becomes smoother and more resilient making wrinkles less noticeable and your complexion appears to glow with more radiance than ever. Ultimune Power Infusing Concentrate is for all women of all ages. It works with your skincare to boost the benefits of your regimen, no matter your concerns.{Environment.NewLine}Benefits: Shiseido's exclusive Ultimune Complex helps boost defensive functions that have declined in Langerhans Cells, the cells that hold the key to promoting skin's multi-defensive power. Now your skin can achieve its greatest beauty potential. Immediately, skin feels full and supple, with a silky-smooth surface. In one week, skin appears to glow more than ever. In 4 weeks: firmness and resilience are improved, making wrinkles less visible. Apply morning and night after cleansing and softening the face. When using other serums, apply Ultimune first to enhance the benefits of the following treatments."));
			Assert.That(rows[2]["LongDescription"], Is.EqualTo("A new chapter in the Flowerbomb story. A true bouquet of flowers that are good enough to eat! With crispy buds that evoke the “morning dew”. This Fragrance is multi-faceted like the bottle it comes in: a mille-feuille of Flowerbomb flowers around freesia, centifolia rose and Sambac jasmine, refreshed by crispy green notes and a touch of mandarin and bergamot."));
		}

        [Test]
        public void Parse_Lines_Starting_With_Quote()
        {
            CsvRow[] rows = Parse($"WEB2100\t000000000060062774\t\"2.5\" SATA SSD Innodisk\t000000000012430001\t",
                csv => csv
                    .NoHeaders()
                    .ChangeDelimiter("\t")
                    .ChangeHasFieldsEnclosedInQuotes(false));

            Assert.That(rows.Length, Is.EqualTo(1));
            Assert.That(rows[0][0], Is.EqualTo("WEB2100"));
            Assert.That(rows[0][1], Is.EqualTo("000000000060062774"));
            Assert.That(rows[0][2], Is.EqualTo("\"2.5\" SATA SSD Innodisk"));
            Assert.That(rows[0][3], Is.EqualTo("000000000012430001"));

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