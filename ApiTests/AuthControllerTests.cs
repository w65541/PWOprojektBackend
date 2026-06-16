using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ApiTests
{
    public class AuthControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthControllerTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetPubKey_ReturnsOk()
        {
            var response = await _client.GetAsync("/Auth/pubkey");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadAsStringAsync();
            body.Should().Contain("BEGIN PUBLIC KEY");
        }

        [Fact]
        public async Task Authenticate_ValidCredentials_ReturnsToken()
        {
            var response = await _client.PostAsync(
                "/Auth?login=admin&pwd=admin123", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var token = await response.Content.ReadAsStringAsync();
            token.Should().NotBeNullOrEmpty();
            // JWT has 3 parts separated by dots
            token.Trim('"').Split('.').Should().HaveCount(3);
        }

        [Fact]
        public async Task Authenticate_InvalidCredentials_Returns401()
        {
            var response = await _client.PostAsync(
                "/Auth?login=admin&pwd=wrongpassword", null);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Authenticate_NonExistentUser_Returns401()
        {
            var response = await _client.PostAsync(
                "/Auth?login=nobody&pwd=anything", null);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
