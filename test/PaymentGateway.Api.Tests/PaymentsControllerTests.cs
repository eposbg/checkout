using System.Net;
using System.Net.Http.Json;

using FluentValidation.Results;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Infrastructure.Persistance;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Application.Abstractions.ExternalServices;
using PaymentGateway.Application.Abstractions.Services;
using PaymentGateway.Application.Models.AcquiringBank;
using PaymentGateway.Application.Services;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    private readonly Random _random = new();

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
            CardNumberLastFour = _random.Next(1111, 9999),
            Currency = "GBP"
        };

        var paymentsRepository = new PaymentsRepository();
        paymentsRepository.Add(payment);

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton(paymentsRepository)))
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
        //    // Arrange
        //    var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        //    var client = webApplicationFactory.CreateClient();

        //    // Act
        //    var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");

        //    // Assert
        //    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        //}
    }


    [Fact]
    public async Task ProcessValidPaymentSuccessfully()
    {
        // Arrange
       // var payment = new PostPaymentRequest
       // {
       //     CardNumber = "111",
       //     ExpiryMonth = 4,
       //     ExpiryYear = 2025,
       //     Currency = "GBP",
       //     Amount = -1,
       //     Cvv = 123
       // };

       // var paymentsRepository = new PaymentsRepository();
       // var mockBankClient = Substitute.For<IAcquiringBankClient>();
       // mockBankClient.ProcessPayment(Arg.Any<PostPaymentRequest>(), Arg.Any<CancellationToken>())
       //.Returns(new BankPaymentResponse
       //{
       //    Authorized = true,
       //    AuthorizationCode = Guid.NewGuid().ToString()
       //});

       // var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
       // var client = webApplicationFactory.WithWebHostBuilder(builder =>
       //     builder.ConfigureServices(services => ((ServiceCollection)services)
       //         .AddScoped<IPaymentService, PaymentService>()
       //         .AddSingleton(mockBankClient)
       //         .AddSingleton(paymentsRepository)

       //         ))
       //     .CreateClient();

       // // Act
       // var response = await client.PostAsJsonAsync($"/api/payments", payment);
       // var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

       // // Assert
       // Assert.Equal(HttpStatusCode.OK, response.StatusCode);
       // Assert.NotNull(paymentResponse);
    }


    [Fact]
    public async Task ProcessInvalidPaymentReturnsValidationErrors()
    {
        // Arrange
        var payment = new PostPaymentRequest
        {
            CardNumber = "111",
            ExpiryMonth=13,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = -1,
            Cvv = 123
        };

        var paymentsRepository = new PaymentsRepository();
        var mockBankClient = Substitute.For<IAcquiringBankClient>();
        mockBankClient.ProcessPayment(Arg.Any<PostPaymentRequest>(), Arg.Any<CancellationToken>())
       .Returns(new BankPaymentResponse
       {
           Authorized = true,
           AuthorizationCode = Guid.NewGuid().ToString()
       });

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
        var errors = await response.Content.ReadFromJsonAsync<List<ValidationFailure>>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(errors);
        Assert.Contains(errors, e => e.PropertyName == "CardNumber");
        Assert.Contains(errors, e => e.PropertyName == "Amount");
        Assert.Contains(errors, e => e.PropertyName == "ExpiryMonth");
        Assert.Contains(errors, e => e.PropertyName == "ExpiryYear");


    }
}