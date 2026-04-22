using System.Net.Http.Json;
using System.Text.Json;
using FixTrading.OrderProcessing.Application.Contracts;

namespace FixTrading.OrderGenerator.Api.Integration.Tests.Fixtures;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly OrderGeneratorApiFactory Factory;
    protected readonly HttpClient Client;

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected IntegrationTestBase(OrderGeneratorApiFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public Task InitializeAsync()
    {
        Factory.ResetFixClient();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task<HttpResponseMessage> PostOrderAsync(SendOrderRequest request)
    {
        return await Client.PostAsJsonAsync("/api/orders", request);
    }

    protected async Task<T?> ReadResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }
}