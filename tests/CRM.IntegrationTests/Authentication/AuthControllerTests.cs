using System.Net.Http.Json;
using CRM.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit.Abstractions;

namespace CRM.IntegrationTests.Authentication;

public class AuthControllerTests : IClassFixture<ApiApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public AuthControllerTests(ApiApplicationFactory factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _output = output;
    }

    [Fact(Skip = "InMemory provider wiring pending for integration environment.")]
    public async Task Login_ShouldReturnTokens_ForDefaultAdminUser()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            UserNameOrEmail = "admin@crm.local",
            Password = "Admin123!"
        });

        var payload = await response.Content.ReadFromJsonAsync<AuthResponse>();

        _output.WriteLine(await response.Content.ReadAsStringAsync());

        response.IsSuccessStatusCode.Should().BeTrue();
        payload.Should().NotBeNull();
        payload!.Tokens.AccessToken.Should().NotBeNullOrEmpty();
        payload.Tokens.RefreshToken.Should().NotBeNullOrEmpty();
        payload.Roles.Should().Contain("Admin");
    }

    private sealed record AuthResponse(Guid UserId, string UserName, string Email, string[] Roles, TokenResponse Tokens);

    private sealed record TokenResponse(string AccessToken, DateTime AccessTokenExpiresAt, string RefreshToken, DateTime RefreshTokenExpiresAt);
}

