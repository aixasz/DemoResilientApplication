
using DotNetConfTh.DemoResilience.CircuitBreakerPattern;

// simple CircuitBreaker
var circuitBreaker = new SimpleCircuitBreaker(failureThreshold: 2, breakDuration: TimeSpan.FromSeconds(3));
await circuitBreaker.RunAsync();


// use resilience pipeline
var resiliencePipeline = new CircuitBreakerPatternPipeline();
await resiliencePipeline.RunAsync();