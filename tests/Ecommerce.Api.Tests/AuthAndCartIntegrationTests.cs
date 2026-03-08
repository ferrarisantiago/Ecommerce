using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Ecommerce.Application.Auth;
using Ecommerce.Application.Cart;

namespace Ecommerce.Api.Tests;

public class AuthAndCartIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthAndCartIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ReturnsCreatedWithJwtToken()
    {
        RegisterRequest request = BuildRegisterRequest();

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/register", request);
        string payload = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Contains("token", payload, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_ReturnsOkWithJwtToken()
    {
        RegisterRequest registerRequest = BuildRegisterRequest();
        HttpResponseMessage registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        LoginRequest loginRequest = new()
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        string payload = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("token", payload, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CartSummary_WithoutToken_ReturnsUnauthorized()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/cart/summary", new CartSummaryRequest
        {
            Items = [new CartItemInput { ProductId = 1, ProductName = "A", UnitPrice = 10m, Quantity = 2 }]
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CartSummary_WithToken_ReturnsCalculatedTotals()
    {
        RegisterRequest registerRequest = BuildRegisterRequest();
        HttpResponseMessage registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        string token = await ExtractTokenAsync(registerResponse);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/cart/summary", new CartSummaryRequest
        {
            Items =
            [
                new CartItemInput { ProductId = 1, ProductName = "A", UnitPrice = 100m, Quantity = 1 },
                new CartItemInput { ProductId = 2, ProductName = "B", UnitPrice = 50m, Quantity = 2 }
            ]
        });

        string json = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(json);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(200m, doc.RootElement.GetProperty("subtotal").GetDecimal());
        Assert.Equal(20m, doc.RootElement.GetProperty("tax").GetDecimal());
        Assert.Equal(220m, doc.RootElement.GetProperty("total").GetDecimal());
    }

    private static RegisterRequest BuildRegisterRequest()
    {
        string email = $"user-{Guid.NewGuid():N}@example.com";
        return new RegisterRequest
        {
            Email = email,
            Password = "Pass123!",
            Role = "User",
            FullName = "Integration Test User"
        };
    }

    private static async Task<string> ExtractTokenAsync(HttpResponseMessage response)
    {
        string json = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("token").GetString() ?? string.Empty;
    }
}
