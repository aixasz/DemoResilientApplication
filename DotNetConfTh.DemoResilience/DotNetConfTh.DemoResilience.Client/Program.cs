using Microsoft.Extensions.Http.Resilience;
using Polly;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("RetryResilientClient", options => 
{
    options.BaseAddress = new Uri("https://localhost:7145");
})
.AddStandardResilienceHandler(builder =>
{
    builder.Retry = new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = 10,
        MaxDelay = TimeSpan.FromSeconds(2),
        Delay = TimeSpan.FromMilliseconds(500),
        BackoffType = DelayBackoffType.Exponential
    };
});

builder.Services.AddHttpClient("CircuitBreakerClient", options =>
{
    options.BaseAddress = new Uri("https://localhost:7145");
})
.AddStandardResilienceHandler(builder =>
{
    builder.Retry = new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = 20,
        MaxDelay = TimeSpan.FromSeconds(3),
        Delay = TimeSpan.FromSeconds(1),
        BackoffType = DelayBackoffType.Linear,
        
    };

    builder.CircuitBreaker = new HttpCircuitBreakerStrategyOptions
    {
        FailureRatio = 0.5,                             // 50% failure rate triggers the breaker
        SamplingDuration = TimeSpan.FromSeconds(20),    // Window of time for failure calculation
        MinimumThroughput = 3,                          // Minimum number of calls before breaker activates
        BreakDuration = TimeSpan.FromSeconds(3),        // Time to keep the circuit open
        
    };
});

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.MapStaticAssets();

app.MapScalarApiReference();
app.MapOpenApi();

app.MapGet("/retry-client", async (HttpContext context, IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient("RetryResilientClient");

    Console.WriteLine("Testing /retry endpoint...");
    try
    {
        var retryResponse = await client.GetStringAsync("/retry");
        Console.WriteLine($"Retry Response: {retryResponse}");

        // Write success response back to the API client
        context.Response.StatusCode = StatusCodes.Status200OK;
        await context.Response.WriteAsync($"Retry succeeded: {retryResponse}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Retry Request Failed: {ex.Message}");

        // Write failure response back to the API client
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync($"Retry failed: {ex.Message}");
    }
});

app.MapGet("/circuit-breaker-client", async (HttpContext context, IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient("CircuitBreakerClient");

    Console.WriteLine("Testing /circuit-breaker endpoint...");
    try
    {
        var circuitBreakerResponse = await client.GetStringAsync("/circuit-breaker");
        Console.WriteLine($"Circuit Breaker Response: {circuitBreakerResponse}");

        // Write success response back to the API client
        context.Response.StatusCode = StatusCodes.Status200OK;
        await context.Response.WriteAsync($"Circuit Breaker succeeded: {circuitBreakerResponse}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Circuit Breaker Request Failed: {ex.Message}");

        // Write failure response back to the API client
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync($"Circuit Breaker failed: {ex.Message}");
    }
});
app.Run();
