using DotNetConfTh.DemoResilience.Extensions;

namespace DotNetConfTh.DemoResilience.RetryPattern;
public class SimpleRetryPattern
{
    public async Task RunAsync()
    {
        Console.WriteLine("Manual Retry Logic:");
        var simulateWork = new SimulateWork();

        await ExecuteWithRetryAsync(simulateWork.ExecuteAsync,
                                    maxRetryAttempts: 3,
                                    delay: TimeSpan.FromSeconds(1));
    }

    public async Task ExecuteWithRetryAsync(Func<Task> action, int maxRetryAttempts, TimeSpan delay)
    {
        int attempt = 0;

        while (true)
        {
            try
            {
                await action();
                return;
            }
            catch (Exception ex) when (attempt < maxRetryAttempts)
            {
                attempt++;
                Console.WriteLine($"Retry {attempt}: {ex.Message}");
                await Task.Delay(delay);
            }
        }
    }
}
