using Database.Dto;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace ApiTests
{
    public class UserControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public UserControllerTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            AuthorizeClient().GetAwaiter().GetResult();
        }

        private async Task AuthorizeClient()
        {
            var response = await _client.PostAsync("/Auth?login=admin&pwd=admin123", null);
            var token = (await response.Content.ReadAsStringAsync()).Trim('"');
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // ── GET /User/getAll ──────────────────────────────────────────
        [Fact]
        public async Task GetUsers_ReturnsOkWithList()
        {
            var response = await _client.GetAsync("/User/getAll");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var users = await response.Content.ReadFromJsonAsync<List<UserDto>>(_json);
            users.Should().NotBeNull();
            users!.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetUsers_ContainsAdminUser()
        {
            var response = await _client.GetAsync("/User/getAll");
            var users = await response.Content.ReadFromJsonAsync<List<UserDto>>(_json);

            users.Should().Contain(u => u.Login == "admin");
        }

        // ── GET /User/get ─────────────────────────────────────────────
        [Fact]
        public async Task GetUser_ExistingId_ReturnsUser()
        {
            var response = await _client.GetAsync("/User/get?id=1");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var user = await response.Content.ReadFromJsonAsync<UserDto>(_json);
            user.Should().NotBeNull();
            user!.Login.Should().Be("admin");
        }

        [Fact]
        public async Task GetUser_NonExistingId_ReturnsBadRequest()
        {
            var response = await _client.GetAsync("/User/get?id=99999");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ── POST /User/add ────────────────────────────────────────────
        [Fact]
        public async Task AddUser_ValidData_ReturnsOk()
        {
            var response = await _client.PostAsync(
                "/User/add?username=newuser&password=pass123&email=newuser@test.com", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AddUser_DuplicateLogin_ReturnsBadRequest()
        {
            var response = await _client.PostAsync(
                "/User/add?username=admin&password=pass123&email=unique@test.com", null);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = await response.Content.ReadAsStringAsync();
            body.Should().Contain("taken");
        }

        [Fact]
        public async Task AddUser_DuplicateEmail_ReturnsBadRequest()
        {
            var response = await _client.PostAsync(
                "/User/add?username=uniquelogin&password=pass123&email=admin@test.com", null);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ── POST /User/archive ────────────────────────────────────────
        [Fact]
        public async Task Archive_ExistingUser_ReturnsOk()
        {
            // First create a user to archive
            await _client.PostAsync(
                "/User/add?username=todelete&password=pass&email=todelete@test.com", null);

            var allResp = await _client.GetAsync("/User/getAll");
            var users = await allResp.Content.ReadFromJsonAsync<List<UserDto>>(_json);
            var toDelete = users!.First(u => u.Login == "todelete");

            var response = await _client.PostAsync(
                $"/User/archive?id={toDelete.Id}&archId=1", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Archive_NonExistingUser_ReturnsBadRequest()
        {
            var response = await _client.PostAsync("/User/archive?id=99999&archId=1", null);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ── POST /User/update ─────────────────────────────────────────
        [Fact]
        public async Task UpdateUser_ValidData_ReturnsOk()
        {
            var response = await _client.PutAsync(
                "/User/update?id=2&email=updated@test.com", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
