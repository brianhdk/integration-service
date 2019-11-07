using Newtonsoft.Json;

namespace Vertica.Integration.Emails.Mandrill.Infrastructure
{
    public abstract class MandrillApiRequest
    {
        [JsonProperty("key")]
        internal string Key { get; set; }
    }
}