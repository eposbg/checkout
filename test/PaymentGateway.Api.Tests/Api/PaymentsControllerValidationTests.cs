using System.Net;
using System.Net.Http.Json;

using FluentValidation.Results;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NSubstitute;

using PaymentGateway.Api.Configuration;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Infrastructure.Persistance;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Application.Abstractions.ExternalServices;
using PaymentGateway.Application.Abstractions.Services;
using PaymentGateway.Application.Models.AcquiringBank;
using PaymentGateway.Application.Services;

namespace PaymentGateway.Api.Tests.Api;

public class PaymentsControllerValidationTests
{
    [Fact]
    public async Task ProcessValidPaymentSuccessfully()
    {
        // Arrange
        var payment = CreateValidPaymentRequest();

        var paymentsRepository = new PaymentsRepository();
        var mockBankClient = Substitute.For<IAcquiringBankClient>();
        mockBankClient.ProcessPayment(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>())
            .Returns(new BankPaymentResponse { Authorized = true, AuthorizationCode = Guid.NewGuid().ToString() });

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services => ((ServiceCollection)services)
                    .AddScoped<IPaymentService, PaymentService>()
                    .AddSingleton(mockBankClient)
                    .AddSingleton(paymentsRepository)
                ))
            .CreateClient();

        // Act
        var response = await client.PostAsJsonAsync($"/api/payments", payment);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
    }
    
    [Theory]
    [InlineData("111")]
    [InlineData("12345678901234567890")]
    [InlineData("abcd1234")]
    public async Task RejectInvalidCardNumber(string cardNumber)
    {
        //Arrange
        var payment = CreateValidPaymentRequest();
        payment.CardNumber = cardNumber;

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder => { }).CreateClient();

        // Act
        var response = await client.PostAsJsonAsync($"/api/payments", payment);
        var errors = await response.Content.ReadFromJsonAsync<List<ValidationFailure>>();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(errors);
        Assert.Contains(errors, e => e.PropertyName == "CardNumber");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(13)]
    public async Task RejectInvalidExpiaryMonth(int expiaryMonth)
    {
        //Arrange
        var payment = CreateValidPaymentRequest();
        payment.ExpiryMonth = expiaryMonth;

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder => { }).CreateClient();

        // Act
        var response = await client.PostAsJsonAsync($"/api/payments", payment);
        var errors = await response.Content.ReadFromJsonAsync<List<ValidationFailure>>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(errors);
        Assert.Contains(errors, e => e.PropertyName == "ExpiryYear");
    }

    [Fact]
    public async Task RejectExpiredCard()
    {
        //Arrange
        var payment = CreateValidPaymentRequest();
        payment.ExpiryYear = DateTime.Now.Year - 1;

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder => { }).CreateClient();

        // Act
        var response = await client.PostAsJsonAsync($"/api/payments", payment);
        var errors = await response.Content.ReadFromJsonAsync<List<ValidationFailure>>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(errors);
        Assert.Contains(errors, e => e.PropertyName == "ExpiryYear");
    }

    [Theory]
    [InlineData("GBP", true)]
    [InlineData("YEN", false)]
    [InlineData("USD", true)]
    [InlineData("EUR", true)]
    [InlineData("CAD", true)]
    public async Task ValidateCurrency(string currency, bool valid)
    {
        //Arrange
        var payment = CreateValidPaymentRequest();
        payment.Currency = currency;
        var paymentSettings = new PaymentSettings { SupportedCurrencies = new[] { "USD", "EUR", "GBP", "CAD" } };

        var mockPaymentService = Substitute.For<IPaymentService>();
        mockPaymentService
            .Process(Arg.Any<PostPaymentRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PostPaymentResponse { });
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .Configure<PaymentSettings>(config =>
                    config.SupportedCurrencies = paymentSettings.SupportedCurrencies)
                .AddSingleton(Options.Create(paymentSettings))
                .AddScoped(_ => mockPaymentService)
            )).CreateClient();

        // Act
        var response = await client.PostAsJsonAsync($"/api/payments", payment);

        // Assert
        Assert.Equal(valid, response.IsSuccessStatusCode);
        if (!response.IsSuccessStatusCode)
        {
            var errors = await response.Content.ReadFromJsonAsync<List<ValidationFailure>>();

            Assert.NotNull(errors);
            Assert.Contains(errors, e => e.PropertyName == "Currency");
        }
    }


    [Fact]
    public async Task RejectNegativeAmount()
    {
        //Arrange
        var payment = CreateValidPaymentRequest();
        payment.Amount = -1;

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder => { }).CreateClient();

        // Act
        var response = await client.PostAsJsonAsync($"/api/payments", payment);
        var errors = await response.Content.ReadFromJsonAsync<List<ValidationFailure>>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(errors);
        Assert.Contains(errors, e => e.PropertyName == "Amount");
    }

    [Theory]
    [InlineData("123", true)]
    [InlineData("1234", true)]
    [InlineData("12345", false)]
    [InlineData("12", false)]
    [InlineData("abc", false)]
    public async Task ValidateCVV(string cvv, bool expect)
    {
        //Arrange
        var payment = CreateValidPaymentRequest();
        payment.Cvv = cvv;

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder => { }).CreateClient();

        // Act
        var response = await client.PostAsJsonAsync($"/api/payments", payment);

        // Assert
        Assert.Equal(expect, response.IsSuccessStatusCode);
        if (!response.IsSuccessStatusCode)
        {
            var errors = await response.Content.ReadFromJsonAsync<List<ValidationFailure>>();

            Assert.NotNull(errors);
            Assert.Contains(errors, e => e.PropertyName == "Cvv");
        }
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