using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using System.Text.Json;


namespace Dnc.ConsoleClient
{
    internal class Program
    {
        private static readonly string clientSecret = "{your-client-secret}";
        private static readonly string clientId = "{your-client-id}";
        private static readonly string tenantId = "{your-tenant-id}";
        private static readonly string[] scopes = ["api://{your-audience}/.default"];
        static async Task Main(string[] args)
        {
            string accessToken = await GetAccessTokenUsingHttpClient();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Press Enter to Acquire Access Token using HttpClient (Credentials Flow)");
            Console.ResetColor();
            Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"************Access Token Using HttpClient***************");
            Console.ResetColor();
            Console.WriteLine($"{accessToken}\n");

            var data = await GetWeatherForecast(accessToken);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Press Enter to call the secure web api");
            Console.ResetColor();
            Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"************Response from the Secured API***************");
            Console.ResetColor();
            Console.WriteLine($"{data}\n");

            string accessTokenUsingMSAL = await GetAccessTokenUsingMSAL();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Press Enter to Acquire Access Token using MSAL.NET (Credentials Flow)");
            Console.ResetColor();
            Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"************Access Token using MSAL.NET***************");
            Console.ResetColor();
            Console.WriteLine($"{accessTokenUsingMSAL}\n");

            var data2 = await GetWeatherForecast(accessToken);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Press Enter to call the secure web api");
            Console.ResetColor();
            Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"************Response from the Secured API***************");
            Console.ResetColor();
            Console.WriteLine($"{data2}");
        }

        //Using HttpClient to acquire token using the client credentials flow.
        private static async Task<string> GetAccessTokenUsingHttpClient()
        {
            using var client = new HttpClient();

            var tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

            var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
            var parameters = new Dictionary<string, string>
            {
                {"client_id", clientId },
                {"client_secret", clientSecret},
                {"scope", scopes[0]},
                {"grant_type", "client_credentials"}
            };
            var content = new FormUrlEncodedContent(parameters);

            request.Content = content;

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}");

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            return doc.RootElement.TryGetProperty("access_token", out var access_token) ?
               access_token.GetString() : throw new InvalidOperationException("access token not found.");
        }

        //Using MSAL.NET to acquire token using the client credentials flow.
        private static async Task<string> GetAccessTokenUsingMSAL()
        {
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
                .Build();

            AuthenticationResult result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            return result.AccessToken;
        }

        private static async Task<string> GetWeatherForecast(string accessToken)
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7157/") };

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return await httpClient.GetStringAsync("WeatherForecast");
        }
    }
}
