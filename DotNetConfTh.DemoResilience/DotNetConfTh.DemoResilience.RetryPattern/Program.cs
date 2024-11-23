using DotNetConfTh.DemoResilience.RetryPattern;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

// Execute manual retry
var simpleRetryPattern = new SimpleRetryPattern();
await simpleRetryPattern.RunAsync();


// Execute resilience pipeline

var resiliencePipeline = new RetryPatternPipeline();
await resiliencePipeline.RunAsync();