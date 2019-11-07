using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Utilities.Extensions.EnumerableExt;

namespace Experiments.Console.Archiving
{
    public static class Demo
    {
        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());

        public static void Run()
        {
            var directory = new DirectoryInfo(@"c:\tmp\archives");

            using (IApplicationContext context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Disable()))
                .Services(services => services
                    .Advanced(advanced => advanced
                        // We'll override where to read RuntimeSettings from - just for demo purposes
                        // It's recommended to configure these in the app.config instead - which is the default behaviour
                        .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings()
                            .Set("FileBasedArchiveService.BaseDirectory", directory.FullName))))))
            {
                if (!directory.Exists)
                    directory.Create();

                directory.GetFiles().ForEach(file => file.Delete());
                directory.GetDirectories().ForEach(subDirectory => subDirectory.Delete(recursive: true));

                IEnumerable<string> randomData = Enumerable.Range(1, 100000)
                    .Select(x => Guid.NewGuid().ToString("B"))
                    .Select(x => $"{x}-{x.GetHashCode()}-{string.Join(string.Empty, Enumerable.Range(1, 100).Select(_ => (char) Random.Next(1, 10000)))}");

                string contents = string.Join(System.Environment.NewLine, randomData);

                var raw = new FileInfo(Path.Combine(directory.FullName, "RAW.txt"));
                
                File.WriteAllText(raw.FullName, contents);
                raw.Refresh();

                var archiveService = context.Resolve<IArchiveService>();

                void Rename(ArchiveCreated archive, CompressionLevel? compressionLevel = null)
                {
                    var compressed = new FileInfo(Path.Combine(directory.FullName, $"{archive.Id}.zip"));

                    double savedPercentage = (((double) raw.Length - compressed.Length) / raw.Length) * 100;
	
                    string destFileName = $"Compression_{(compressionLevel.HasValue ? compressionLevel.ToString() : "NotSpecified")}_{savedPercentage:0}{compressed.Extension}";

                    compressed.MoveTo(Path.Combine(directory.FullName, destFileName));
                }

                Rename(archiveService.ArchiveText("RandomData", contents));

                foreach (CompressionLevel compressionLevel in Enum.GetValues(typeof(CompressionLevel)))
                {
                    ArchiveCreated archive = archiveService.ArchiveText("RandomData", contents, options => options
                        .Compression(compressionLevel));

                    Rename(archive, compressionLevel);
                }
            }
        }
    }
}