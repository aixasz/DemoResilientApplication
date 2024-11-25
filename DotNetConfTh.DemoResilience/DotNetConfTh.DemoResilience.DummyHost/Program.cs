var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Simulate intermittent failures for retry testing
app.MapGet("/retry", async context =>
{

    var random = new Random();
    if (random.Next(0, 2) == 0) // Fail 50% of the time
    {
        context.Response.StatusCode = 500;
        Console.WriteLine("Simulated transient failure");
        await context.Response.WriteAsync("Simulated transient failure");
    }
    else
    {
        Console.WriteLine("Success after retry");
        await context.Response.WriteAsync("Success after retry");
    }
});

// Simulate persistent failures followed by success for circuit breaker testing
app.MapGet("/circuit-breaker", async context =>
{

    var random = new Random();
    if (random.Next(0, 2) == 0) // Fail 50% of the time
    {
        context.Response.StatusCode = 500; // Simulated failure
        Console.WriteLine("Simulated failure");
        await context.Response.WriteAsync("Simulated failure");
    }
    else
    {
        context.Response.StatusCode = 200; // Success
        Console.WriteLine("Success after circuit breaker");
        await context.Response.WriteAsync("Success after circuit breaker");
    }
});

await app.RunAsync();
