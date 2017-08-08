using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using Castle.MicroKernel;
using Vertica.Integration;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Perfion;
using Vertica.Integration.Perfion.Infrastructure;
using Vertica.Integration.Perfion.Infrastructure.Client;
using Vertica.Utilities.Collections;
using Vertica.Utilities.Extensions.EnumerableExt;

namespace Experiments.Console.Perfion
{
    public static class Demo
    {
        public static void Run()
        {
            using (IApplicationContext context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Disable()))
                .UsePerfion(perfion => perfion
                    // Setup the default connection based on a ConnectionString-element in app.config: <add name="Perfion" connectionString="http://perfion-01/Perfion/GetData.asmx" />
                    .DefaultConnection(ConnectionString.FromName("Perfion.APIService.Url"), perfionClient => perfionClient
                        // Setup global archiving - all queries will be archived.
                        .EnableArchiving())
                    // Setup the default connection allowing further customization of this default connection, see class "OverrideDefaultConnection" for more details
                    .DefaultConnection(new OverrideDefaultConnection(ConnectionString.FromText("http://perfion-01/Perfion/GetData.asmx")))
                    // Adds an additional connection to any Perfion API service
                    .AddConnection(new AnotherPerfionConnection(ConnectionString.FromText("http://perfion-02/Perfion/GetData.asmx")), perfionClient => perfionClient
                        // Setup global archiving - specifying that archives should expire after 10 days.
                        .EnableArchiving(options => options.ExpiresAfterDays(10))))))
            {
                // Connect to default Perfion Service
                IPerfionClient defaultPerfionClient = context.Resolve<IPerfionClient>();

                // Queries data from Perfion - override the default archiving option, in this case archiving is disabled.
                PerfionXml categoriesXml = defaultPerfionClient.Query(@"
<Query>
    <Select languages='en'>
        <Feature id='**' />
    </Select>
    <From id='Category'/>
</Query>", archive => archive.Disable());

                Tree<PerfionXml.Component, string, int> categories = categoriesXml.Components("Category")
                    .ToTree(x => x.Id, (x, p) => x.ParentId.HasValue ? p.Value(x.ParentId.Value) : p.None, x => x.Name());

                var treeVisualization = new StringBuilder();

                foreach (var level1Category in categories)
                {
                    treeVisualization.AppendLine(level1Category.Model);

                    foreach (var level2Category in level1Category)
                    {
                        treeVisualization.AppendLine($"-- {level2Category.Model}");
                    }
                }

                string s = treeVisualization.ToString();

                /*
Motor Vechicles
-- Cars
-- Busses
Clothes
-- Pants
                */

                // Connects to the other Perfion Service
                IPerfionClient anotherPerfionClient = context.Resolve<IPerfionClientFactory<AnotherPerfionConnection>>().Client;

                PerfionXml productsXml = anotherPerfionClient.Query(@"
<Query>
    <Select languages='dan' maxCount='2'>
        <Feature id='**' />
    </Select>
    <From id='Product' />
    <Where>
		<Clause id='MyFeature' operator='=' value='MyValue' />
    </Where>
</Query>");

                foreach (PerfionXml.Component product in productsXml.Components("Product"))
                {
                    PerfionXml.Component parentCategory = product.FindRelation("Category");

                    PerfionXml.Image image = product.GetImages("MainImage").FirstOrDefault();

                    if (image != null)
                    {
                        // download RAW
                        byte[] raw = image.Download();
                        File.WriteAllBytes(Path.Combine(@"C:\tmp\perfion\", image.Name), raw);

                        // download thumb (100x100)
                        byte[] thumbnail = image.Download(new NameValueCollection {{"size", "100x100"}});
                        File.WriteAllBytes(Path.Combine(@"c:\tmp\perfion\thumbs\", image.Name), thumbnail);
                    }

                    PerfionXml.File[] drawings = product.GetFiles("SketchDrawing");

                    foreach (var drawing in drawings)
                    {
                        string fileName = drawing.Element.AttributeOrEmpty("string").Value;
                        File.WriteAllBytes($@"c:\tmp\perfion\{fileName}", drawing.Download());
                    }

                    byte[] report = product.DownloadPdfReport("Datasheet", "dan");
                    File.WriteAllBytes($@"c:\tmp\perfion\{product.Name()}.pdf", report);
                }
            }
        }
    }

    public class OverrideDefaultConnection : Connection
    {
        public OverrideDefaultConnection(ConnectionString connectionString)
            : base(connectionString)
        {
        }

        protected override void ConfigureBinding(BasicHttpBinding binding, IKernel kernel)
        {
            base.ConfigureBinding(binding, kernel);
        }

        protected override void ConfigureClientCredentials(ClientCredentials clientCredentials, IKernel kernel)
        {
            base.ConfigureClientCredentials(clientCredentials, kernel);
        }
    }

    public class AnotherPerfionConnection : Connection
    {
        public AnotherPerfionConnection(ConnectionString connectionString)
            : base(connectionString)
        {
        }

        protected override WebClient CreateWebClient(IKernel kernel)
        {
            WebClient client = base.CreateWebClient(kernel);

            client.SetBasicAuthentication("username", "password");

            return client;
        }
    }
}