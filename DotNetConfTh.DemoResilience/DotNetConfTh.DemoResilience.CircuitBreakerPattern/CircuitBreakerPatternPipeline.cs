using DotNetConfTh.DemoResilience.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using Polly.Retry;
using System;
using System.Diagnostics;

namespace DotNetConfTh.DemoResilience.CircuitBreakerPattern;

public class CircuitBreakerPatternPipeline
{
    public async Task RunAsync()
    {
        var services = ConfigureServiceCollection();
        await using var serviceProvider = services.BuildServiceProvider();

        var pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
        var circuitBreakerPipeline = pipelineProvider.GetPipeline("circuitBreakerPipeline");

        // Execute the pipeline
        var simulateWork = new SimulateWork();

        // Execute the pipeline
        Console.WriteLine("Circuit Breaker Pipeline Execution:");
        try
        {
            await circuitBreakerPipeline.ExecuteAsync(async _ => await simulateWork.ExecuteAsync());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Operation terminated: {ex.Message}");
            throw;
        }
    }




    private static ServiceCollection ConfigureServiceCollection()
    {
        var services = new ServiceCollection();

        const string key = "circuitBreakerPipeline";

        services.AddResiliencePipeline(key, static builder =>
        {

            builder.AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                Delay = TimeSpan.FromSeconds(1),                // 1 second delay between retries
                BackoffType = DelayBackoffType.Constant,        // Constant delay
                MaxRetryAttempts = 6,                           // Retry up to 6 times
                MaxDelay = TimeSpan.FromSeconds(1),             // Maximum total delay
                UseJitter = false                               // No jitter for simplicity
            });

            builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                FailureRatio = 0.5,                             // 50% failure rate triggers the breaker
                SamplingDuration = TimeSpan.FromSeconds(10),    // Window of time for failure calculation
                MinimumThroughput = 2,                          // Minimum number of calls before breaker activates
                BreakDuration = TimeSpan.FromSeconds(3),         // Time to keep the circuit open
                                                                 // Log when the circuit closes (resets to normal state)
                OnClosed = args =>
                {
                    Console.WriteLine("Circuit breaker reset to CLOSED state.");
                    return default;
                },

                // Log when the circuit opens (stops accepting requests)
                OnOpened = args =>
                {
                    Console.WriteLine("Circuit breaker transitioned to OPEN state. All requests will be rejected.");
                    return default;
                },

                // Log when the circuit transitions to Half-Open (testing state)
                OnHalfOpened = args =>
                {
                    Console.WriteLine("Circuit breaker transitioning to HALF-OPEN state. Test requests will be allowed.");
                    return default;
                }
            });
        });

        services.AddResilienceEnricher();
        return services;
    }
}