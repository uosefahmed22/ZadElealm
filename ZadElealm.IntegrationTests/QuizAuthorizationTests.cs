using Xunit;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ZadElealm.IntegrationTests
{
    public class QuizAuthorizationTests : IClassFixture<ZadElealmApiFactory>
    {
        private readonly ZadElealmApiFactory _factory;

        public QuizAuthorizationTests(ZadElealmApiFactory factory)
        {
            _factory = factory;
        }

        private HttpClient CreateClientWithoutRedirects()
        {
            return _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost")
            });
        }

        [Fact]
        public async Task CreateQuiz_WithoutToken_IsRejectedAsUnauthorized()
        {
            var client = CreateClientWithoutRedirects();

            var response = await client.PostAsJsonAsync("/api/Quiz/create",
                new { name = "t", description = "t", courseId = 1, questions = Array.Empty<object>() });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateQuiz_WithUserRole_IsForbidden()
        {
            var client = CreateClientWithoutRedirects();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _factory.GenerateToken("User"));

            var response = await client.PostAsJsonAsync("/api/Quiz/create",
                new { name = "t", description = "t", courseId = 1, questions = Array.Empty<object>() });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetQuiz_WithUserRole_PassesAuthorization()
        {
            var client = CreateClientWithoutRedirects();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _factory.GenerateToken("User"));

            var response = await client.GetAsync("/api/Quiz/1");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
