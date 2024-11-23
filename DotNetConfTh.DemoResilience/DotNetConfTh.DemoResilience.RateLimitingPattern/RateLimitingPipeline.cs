
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.RateLimiting;
using Polly.Registry;
using System.Threading.RateLimiting;

namespace DotNetConfTh.DemoResilience.RateLimitingPattern;

public class RateLimitingPipeline
{
    public async Task RunAsync()
    {
        var services = ConfigureServiceCollection();
        await using var serviceProvider = services.BuildServiceProvider();

        var pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
        var rateLimitingPipeline = pipelineProvider.GetPipeline("rateLimitingPipeline");

        Console.WriteLine("Rate Limiting Pipeline Execution:");

        // Simulate 
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await rateLimitingPipeline.ExecuteAsync(async _ =>
                    {
                        Console.WriteLine($"Processing request {i + 1} at {DateTime.UtcNow:hh:mm:ss.fff}");
                        await Task.Delay(200); // Simulate work duration
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Request {i + 1} was rejected: {ex.Message}");
                }
            }));

            if (i % 2 == 0) await Task.Delay(50); // Burst simulation
        }

        // Wait for all tasks to complete
        await Task.WhenAll(tasks);
    }

    private static ServiceCollection ConfigureServiceCollection()
    {
        var services = new ServiceCollection();

        const string key = "rateLimitingPipeline";

        services.AddResiliencePipeline(key, builder =>
        {
            builder.AddRateLimiter(new RateLimiterStrategyOptions
            {
                // Default concurrency limiter options
                DefaultRateLimiterOptions = new ConcurrencyLimiterOptions
                {

                    PermitLimit = 3,                                                // Allow up to 3 requests per window
                    QueueLimit = 1,                                                 // Queue up to 1 requests
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                },
                OnRejected = args =>
                {
                    // Log rejection events
                    Console.WriteLine($"Request rejected by rate limiter at {DateTime.UtcNow:hh:mm:ss.fff}");
                    return ValueTask.CompletedTask;
                }
            });
        });

        return services;
    }
}