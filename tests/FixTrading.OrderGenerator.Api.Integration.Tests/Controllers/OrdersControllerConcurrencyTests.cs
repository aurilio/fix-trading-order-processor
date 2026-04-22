using System.Net;
using FluentAssertions;
using FixTrading.OrderGenerator.Api.Integration.Tests.Fixtures;
using FixTrading.OrderProcessing.Application.Contracts;
using FixTrading.OrderProcessing.Domain.Entities;
using NSubstitute;

namespace FixTrading.OrderGenerator.Api.Integration.Tests.Controllers;

[Collection(nameof(IntegrationTestCollection))]
public class OrdersControllerConcurrencyTests : IntegrationTestBase
{
    public OrdersControllerConcurrencyTests(OrderGeneratorApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateOrder_MultipleConcurrentRequests_ShouldAllSucceed()
    {
        // Arrange
        const int numberOfRequests = 10;
        var requests = Enumerable.Range(0, numberOfRequests)
            .Select(_ => OrderRequestBuilder.Valid())
            .ToList();

        // Act
        var tasks = requests.Select(PostOrderAsync);
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));

        var results = await Task.WhenAll(
            responses.Select(r => ReadResponseAsync<SendOrderResponse>(r)));

        var clOrdIds = results.Select(r => r!.ClOrdId).ToList();
        clOrdIds.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task CreateOrder_MultipleRequests_ShouldCallFixClientForEach()
    {
        // Arrange
        const int numberOfRequests = 5;

        // Act
        for (int i = 0; i < numberOfRequests; i++)
        {
            await PostOrderAsync(OrderRequestBuilder.Valid());
        }

        // Assert
        await Factory.MockFixClient.Received(numberOfRequests)
            .SendNewOrderAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }
}