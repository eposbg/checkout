using System.Net;
using System.Net.Http.Json;
using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using NuGet.Frameworks;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Infrastructure.Persistance;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Application.Abstractions.ExternalServices;
using PaymentGateway.Application.Abstractions.Repositories;
using PaymentGateway.Application.Models.AcquiringBank;
using PaymentGateway.Domain.Exceptions;

namespace PaymentGateway.Api.Tests.Api;

public class PaymentsControllerTests
{
    private readonly Random _random = new();


    [Theory]
    [InlineData(true, "Authorized")]
    [InlineData(false, "Declined")]
    public async Task ProcessValidPayment(bool bankAuthorized, string expectedStatus)
    {
        // Arrange
        var repository = new PaymentsRepository();
        var mockAcquiringBankClient = Substitute.For<IAcquiringBankClient>();
        mockAcquiringBankClient.ProcessPayment(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>())
            .Returns(new BankPaymentResponse { Authorized = bankAuthorized });

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory
            .WithWebHostBuilder(
                builder =>
                    builder
                        .ConfigureServices(services => ((ServiceCollection)services)
                        .AddScoped(x => mockAcquiringBankClient)
                        .AddSingleton(repository)
                ))
            .CreateClient();

        var payload = CreateValidPaymentRequest();

        // Act 
        var response = await client.PostAsJsonAsync("/api/payments", payload);
        var result = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(expectedStatus, result?.Status);
    }

    [Fact]
    public async Task PaymentProcessReturns503OnBankServiceUnavalable()
    {
        // Arrange
        var repository = new PaymentsRepository();
        var mockAcquiringBankClient = Substitute.For<IAcquiringBankClient>();
        mockAcquiringBankClient.ProcessPayment(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>())
                    .ThrowsAsync(new BankServiceUnavailableException());

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory
            .WithWebHostBuilder(
                builder =>
                    builder
                        .ConfigureServices(services => ((ServiceCollection)services)
                        .AddScoped(x => mockAcquiringBankClient)
                        .AddSingleton(repository)
                ))
            .CreateClient();

        var payload = CreateValidPaymentRequest();

        // Act 
        var response = await client.PostAsJsonAsync("/api/payments", payload);

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }


    [Fact]
    public async Task RetrievesAPaymentSuccessfully()
    {
        // Arrange
        var payment = new Domain.Entries.Payment
        {
            Id = Guid.NewGuid(),
            ExpiryYear = _random.Next(2023, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumberLastFour = _random.Next(1111, 9999).ToString(),
            Currency = "GBP"
        };

        var mockPaymentRepository = Substitute.For<IPaymentsRepository>();
        mockPaymentRepository.Get(Arg.Any<Guid>()).Returns(payment);


        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton(mockPaymentRepository)))
            .CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
    }

    [Fact]
    public async Task Returns404IfPaymentNotFound()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    private PostPaymentRequest CreateValidPaymentRequest()
    {
        return new PostPaymentRequest
        {
            CardNumber = "2222405343248877",
            ExpiryMonth = DateTime.Now.Month,
            ExpiryYear = DateTime.Now.Year + 1,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123"
        };
    }
}