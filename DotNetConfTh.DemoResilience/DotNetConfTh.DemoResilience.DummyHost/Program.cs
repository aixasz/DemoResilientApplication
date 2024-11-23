var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Simulate intermittent failures for retry testing
app.MapGet("/retry", async context =>
{
    var random = new Random();
    if (random.Next(0, 2) == 0) // Fail 50% of the time
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Simulated transient failure");
    }
    else
    {
        await context.Response.WriteAsync("Success after retry");
    }
});


// Track the number of attempts for the circuit breaker endpoint
int circuitBreakerFailureCount = 0;

// Simulate persistent failures followed by success for circuit breaker testing
app.MapGet("/circuit-breaker", async context =>
{
    circuitBreakerFailureCount++;

    // Fail the first 3 requests, then succeed
    if (circuitBreakerFailureCount <= 3)
    {
        context.Response.StatusCode = 500; // Simulated persistent failure
        await context.Response.WriteAsync($"Simulated persistent failure (Attempt {circuitBreakerFailureCount})");
    }
    else
    {
        await context.Response.WriteAsync("Success after circuit breaker");
    }
});

await app.RunAsync();
