using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PaymentGateway.Api.HealthChecks;
using PaymentGateway.Api.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using PaymentGateway.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<PostPaymentRequestValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var bankSimulatorBaseAddress = builder.Configuration["BankSimulator:BaseAddress"] ?? "http://localhost:8080";
var bankSimulatorUri = new Uri(bankSimulatorBaseAddress);
builder.Services.AddHttpClient<IBankSimulatorClient, BankSimulatorClient>(client =>
{
    client.BaseAddress = bankSimulatorUri;
});
builder.Services.AddHttpClient("BankSimulator", client => client.BaseAddress = bankSimulatorUri);

builder.Services.AddSingleton<IPaymentsRepository, PaymentsRepository>();
builder.Services.AddHealthChecks()
    .AddCheck<BankSimulatorHealthCheck>("bank_simulator", tags: new[] { "ready" });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = c => !c.Tags.Contains("ready") });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = _ => true });

app.Run();
