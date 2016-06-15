using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using Vertica.Integration.Infrastructure.Email;

namespace Vertica.Integration.CampaignMonitor
{
    public class CampaignMonitorEmailService : IEmailService
    {
	    public void Send(EmailTemplate template, params string[] recipients)
	    {
		    throw new NotImplementedException();
		    //if (!HasRecipients(email))
		    //	return;

		    //email.From = email.From.NullIfEmpty() ?? new MailAddress(_settings.FromEmailAddress).ToString();

		    //try
		    //{
		    //	using (var client = new HttpClient())
		    //	{
		    //		byte[] authentication = Encoding.ASCII.GetBytes(string.Concat(_settings.CampaignMonitorApiKey, ":"));
		    //		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authentication));

		    //		HttpResponseMessage response = client.PostAsJsonAsync(
		    //			string.Concat("https://api.createsend.com/api/v3.1/transactional/classicEmail/send?clientID=", _settings.CampaignMonitorClientId),
		    //			email).Result;

		    //		if (!response.IsSuccessStatusCode)
		    //			throw new InvalidOperationException($"Unable to send e-mail through Campaign Monitor. StatusCode: {response.StatusCode}, Response: {GetResponseBody(response.Content)}");
		    //	}
		    //}
		    //catch (Exception ex)
		    //{
		    //	throw new InvalidOperationException("Unable to send e-mail through Campaign Monitor.", ex);
		    //}
	    }
		
		//private static bool HasRecipients(TransactionalEmail request)
		//{
		//	if (request.To != null && request.To.Length > 0)
		//		return true;

		//	return request.Bcc != null && request.Bcc.Length > 0;
		//}

		//private static string GetResponseBody(HttpContent content)
		//{
		//	return content.ReadAsStringAsync().Result;
		//}
	}
}
