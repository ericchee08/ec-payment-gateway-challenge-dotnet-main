using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PaymentGateway.Api.HealthChecks;
using PaymentGateway.Api.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using PaymentGateway.Api.Validators;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

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

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestPath", httpContext.Request.Path);
        diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
        diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
    };
    
    if (!app.Environment.IsDevelopment())
    {
        options.GetLevel = (httpContext, elapsed, ex) =>
        {
            if (ex != null) 
            {
                return Serilog.Events.LogEventLevel.Error;
            }

            var path = httpContext.Request.Path.Value ?? string.Empty;

            if (path.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
            {
                return Serilog.Events.LogEventLevel.Verbose;
            }

            if (httpContext.Response.StatusCode > 399 || elapsed > 1000)
            {
                return Serilog.Events.LogEventLevel.Information;
            }

            return Serilog.Events.LogEventLevel.Verbose;
        };
    }
});

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
