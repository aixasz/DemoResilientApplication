using System.Diagnostics;

namespace DotNetConfTh.DemoResilience.Extensions;

[DebuggerStepThrough]
public class SimulateWork
{
    private int attemptCount;

    public SimulateWork() => attemptCount = 0;

    public async Task ExecuteAsync()
    {
        attemptCount++;
        Console.WriteLine($"Attempt {attemptCount}: Executing...");
        if (attemptCount < 3)
        {
            throw new Exception($"Simulated failure on attempt {attemptCount}");
        }
        Console.WriteLine("Success on attempt " + attemptCount);
    }
}