
using DotNetConfTh.DemoResilience.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;
using Polly.Retry;

namespace DotNetConfTh.DemoResilience.RetryPattern;
public class RetryPatternPipeline
{
    public async Task RunAsync()
    {
        var services = ConfigureServiceCollection();
        await using var serviceProvider = services.BuildServiceProvider();

        var pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
        var retryPipeline = pipelineProvider.GetPipeline("retryPipeline");

        // Execute the pipeline
        var simulateWork = new SimulateWork();
        Console.WriteLine("Resilience Pipeline Execution:");
        await retryPipeline.ExecuteAsync(async _ => await simulateWork.ExecuteAsync());
    }

    private static ServiceCollection ConfigureServiceCollection()
    {
        var services = new ServiceCollection();

        const string key = "retryPipeline";

        services.AddResiliencePipeline(key, static builder =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Linear,
                MaxRetryAttempts = 3,
                MaxDelay = TimeSpan.FromSeconds(3),
                UseJitter = false
            });
        });

        services.AddResilienceEnricher();
        return services;
    }
}
