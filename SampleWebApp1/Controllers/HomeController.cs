using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SampleWebApp1.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> About()
        {
            AuthenticationContext authContext = new AuthenticationContext(Startup.Authority);
            ClientCredential clientCredential = new ClientCredential(Startup.ClientId, Startup.ClientSecret);

            AuthenticationResult authResult = null;
            string userObjectId = (this.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            if (null != userObjectId)
            {
                UserCredential userCredential = new UserCredential(userObjectId);

                // Delegated User Identity
                UserIdentifier userIdentifier = new UserIdentifier(userObjectId, UserIdentifierType.UniqueId);
                //authResult = await authContext.AcquireTokenAsync(Startup.TelemetryApiAppIdUri, Startup.ClientId, new Uri(Startup.TelemetryApiAppIdUri), null, userIdentifier); // Fails with NotImplementedException.
                authResult = await authContext.AcquireTokenAsync(Startup.TelemetryApiAppIdUri, Startup.ClientId, userCredential); // Fails with AdalException: 'Unknown user type'.
            }
            else
            {
                // Application Identity
                authResult = await authContext.AcquireTokenAsync(Startup.TelemetryApiAppIdUri, clientCredential);
            }

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, @"https://localhost:44373/api/ping/secure");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
            HttpResponseMessage response = await client.SendAsync(request);

            string message;
            if (response.IsSuccessStatusCode)
            {
                message = await response.Content.ReadAsStringAsync();
            }
            else
            {
                message = "Ping Failed";
            }

            ViewData["Message"] = message;

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
