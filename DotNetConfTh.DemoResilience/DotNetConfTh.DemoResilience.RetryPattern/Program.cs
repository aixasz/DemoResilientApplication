using DotNetConfTh.DemoResilience.RetryPattern;

// Execute manual retry
//var simpleRetryPattern = new SimpleRetryPattern();
//await simpleRetryPattern.RunAsync();


// Execute resilience pipeline

var resiliencePipeline = new RetryPatternPipeline();
await resiliencePipeline.RunAsync();