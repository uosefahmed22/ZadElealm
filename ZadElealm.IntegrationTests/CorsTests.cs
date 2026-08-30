using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace ZadElealm.IntegrationTests
{
    public class CorsTests : IClassFixture<ZadElealmApiFactory>
    {
        private readonly HttpClient _client;
        private readonly ZadElealmApiFactory _factory;

        public CorsTests(ZadElealmApiFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task PreflightRequest_FromConfiguredOrigin_ReturnsNoContentWithCorsHeaders()
        {
            var request = new HttpRequestMessage(HttpMethod.Options, "/api/Account/login");
            request.Headers.Add("Origin", "https://zad-elealm.netlify.app");
            request.Headers.Add("Access-Control-Request-Method", "POST");
            request.Headers.Add("Access-Control-Request-Headers", "Content-Type");

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Contains("https://zad-elealm.netlify.app", response.Headers.GetValues("Access-Control-Allow-Origin"));
            Assert.Contains("POST", response.Headers.GetValues("Access-Control-Allow-Methods"));
        }

        [Fact]
        public async Task PreflightRequest_FromLocalAngularOrigin_ReturnsNoContentWithCorsHeaders()
        {
            var request = new HttpRequestMessage(HttpMethod.Options, "/api/Account/login");
            request.Headers.Add("Origin", "http://localhost:4200");
            request.Headers.Add("Access-Control-Request-Method", "POST");

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Contains("http://localhost:4200", response.Headers.GetValues("Access-Control-Allow-Origin"));
        }
    }
}
