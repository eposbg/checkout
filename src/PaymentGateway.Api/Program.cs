using FluentValidation;

using PaymentGateway.Api.Configuration;
using PaymentGateway.Api.Infrastructure.ExternalServices;
using PaymentGateway.Api.Infrastructure.Persistance;
using PaymentGateway.Api.Validators;
using PaymentGateway.Application.Abstractions.ExternalServices;
using PaymentGateway.Application.Abstractions.Repositories;
using PaymentGateway.Application.Abstractions.Services;
using PaymentGateway.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<PaymentSettings>(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IAcquiringBankClient, AcquiringBankClient>();
builder.Services.AddSingleton<IPaymentsRepository, PaymentsRepository>();

builder.Services.AddValidatorsFromAssemblyContaining<PaymentRequestValidator>();

builder.Services.AddHttpClient("BankSimulator", client =>
{
    client.BaseAddress = new Uri("http://localhost:8080/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
