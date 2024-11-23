using DotNetConfTh.DemoResilience.Extensions;

namespace DotNetConfTh.DemoResilience.CircuitBreakerPattern;

public class SimpleCircuitBreaker
{
    private readonly int failureThreshold;
    private readonly TimeSpan breakDuration;

    private int failureCount;
    private CircuitState state = CircuitState.Closed;
    private DateTime lastFailureTime;

    public SimpleCircuitBreaker(int failureThreshold, TimeSpan breakDuration)
    {
        this.failureThreshold = failureThreshold;
        this.breakDuration = breakDuration;
    }

    public async Task RunAsync()
    {
        var simulateWork = new SimulateWork();
        Console.WriteLine("Starting Circuit Breaker Example:");

        // retry 
        for (int i = 0; i < 5; i++) // Increased loop count for better testing
        {
            try
            {
                await ExecuteAsync(simulateWork.ExecuteAsync);
            }
            catch
            {

            }

            // Add delay to simulate time between requests
            await Task.Delay(1000); // 1-second delay between attempts
        }
    }

    public async Task ExecuteAsync(Func<Task> action)
    {
        // Handle the Open state
        if (state == CircuitState.Open)
        {
            if (DateTime.UtcNow - lastFailureTime > breakDuration)
            {
                Console.WriteLine("Circuit breaker transitioning to HALF-OPEN state. Test requests will be allowed.");
                state = CircuitState.HalfOpen;
            }
            else
            {
                Console.WriteLine("Circuit breaker transitioned to OPEN state. All requests will be rejected.");
                throw new Exception("Circuit is Open. Request denied.");
            }
        }

        // Handle the Half-Open state
        if (state == CircuitState.HalfOpen)
        {
            try
            {
                await action(); // Test the action
                Reset(); // Reset to Closed state on success
            }
            catch (Exception ex)
            {
                // Transition back to Open and start the break timer
                state = CircuitState.Open;
                lastFailureTime = DateTime.UtcNow;
                throw;
            }
            return; // Ensure no further execution in Half-Open state
        }

        // Handle the Closed state
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Operation failed: {ex.Message}");
            HandleFailure(); // Increment failure count and potentially transition to Open
            throw; // Re-throw exception to indicate failure
        }
    }

    private void Reset()
    {
        Console.WriteLine("Circuit breaker reset to Closed state.");
        failureCount = 0;
        state = CircuitState.Closed;
    }

    private void HandleFailure()
    {
        failureCount++;
        if (failureCount >= failureThreshold)
        {
            Console.WriteLine("Failure threshold reached. Circuit breaker opening...");
            state = CircuitState.Open;
            lastFailureTime = DateTime.UtcNow; // Start break timer
        }
    }

    private enum CircuitState
    {
        Closed,
        Open,
        HalfOpen
    }
}
