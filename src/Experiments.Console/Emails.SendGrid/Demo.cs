using Vertica.Integration;
using Vertica.Integration.Emails.SendGrid;
using Vertica.Integration.Emails.SendGrid.Infrastructure;
using Vertica.Integration.Infrastructure.Email;

namespace Experiments.Console.Emails.SendGrid
{
    public static class Demo
    {
        public static void Run()
        {
            using (var context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Disable()))
                .Services(services => services
                    .Advanced(advanced => advanced
                        // We'll override where to read RuntimeSettings from - just for demo purposes
                        // It's recommended to configure these in the app.config instead - which is the default behaviour
                        .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings()
                            // Specify your API key
                            .Set("SendGrid.ApiKey", @""))))
                .UseSendGrid(sendGrid => sendGrid
                    .ConfiguredBy(by => by.RuntimeSettings())
                    .ReplaceEmailService()
                )
            ))
            {
                var email = new TextBasedEmailTemplate("Subject")
                    .WriteLine("Test");

                context.Resolve<IEmailService>().Send(email, "bhk@vertica.dk");
            }
        }
    }

    public class MyImplementation : ISendGridSettingsProvider
    {
        public SendGridSettings Get()
        {
            return new SendGridSettings
            {
                ApiKey = "TODO"
            };
        }
    }
}