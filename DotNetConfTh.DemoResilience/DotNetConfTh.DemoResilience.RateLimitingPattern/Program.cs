using DotNetConfTh.DemoResilience.RateLimitingPattern;

var rateLimitingPipeline = new RateLimitingPipeline();
await rateLimitingPipeline.RunAsync();
