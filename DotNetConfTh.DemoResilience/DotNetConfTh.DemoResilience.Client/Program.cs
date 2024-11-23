using Microsoft.Extensions.Http.Resilience;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("RetryResilientClient")
    .AddStandardResilienceHandler(builder =>
    {
        builder.Retry = new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            MaxDelay = TimeSpan.FromSeconds(2),
            Delay = TimeSpan.FromMilliseconds(500),
            BackoffType = DelayBackoffType.Exponential
        };
    });

builder.Services.AddHttpClient("CircuitBreakerClient")
    .AddStandardResilienceHandler(builder =>
    {
        builder.Retry = new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 5,
            MaxDelay = TimeSpan.FromSeconds(3),
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Linear
        };

        builder.CircuitBreaker = new HttpCircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,                             // 50% failure rate triggers the breaker
            SamplingDuration = TimeSpan.FromSeconds(10),    // Window of time for failure calculation
            MinimumThroughput = 2,                          // Minimum number of calls before breaker activates
            BreakDuration = TimeSpan.FromSeconds(3),        // Time to keep the circuit open
        };
    });

var app = builder.Build();

app.MapGet("/retry-client", async (IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient("RetryResilientClient");

    Console.WriteLine("Testing /retry endpoint...");
    try
    {
        var retryResponse = await client.GetStringAsync("http://localhost:7145/retry");
        Console.WriteLine($"Retry Response: {retryResponse}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Retry Request Failed: {ex.Message}");
    }
});

app.MapGet("/circuit-breaker-client", async (IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient("RetryResilientClient");

    Console.WriteLine("Testing /retry endpoint...");
    try
    {
        var retryResponse = await client.GetStringAsync("http://localhost:7145/retry");
        Console.WriteLine($"Retry Response: {retryResponse}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Retry Request Failed: {ex.Message}");
    }
});

app.Run();
