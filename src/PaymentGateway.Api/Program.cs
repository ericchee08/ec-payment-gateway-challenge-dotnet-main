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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
