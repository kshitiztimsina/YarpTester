using System;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Polly.CircuitBreaker;

// Create HttpClient
var httpClient = new HttpClient();

// Define a circuit breaker policy for async tasks
AsyncCircuitBreakerPolicy circuitBreakerPolicy = Policy
    .Handle<HttpRequestException>()
    .Or<TaskCanceledException>()
    .CircuitBreakerAsync(
        exceptionsAllowedBeforeBreaking: 2,
        durationOfBreak: TimeSpan.FromSeconds(5),
        onBreak: (exception, timespan) =>
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Circuit opened! Blocking requests for {timespan.TotalSeconds}s. Reason: {exception.Message}");
            Console.ResetColor();
        },
        onReset: () =>
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Circuit closed. Requests flow normally.");
            Console.ResetColor();
        },
        onHalfOpen: () =>
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Circuit is half-open. Testing the service...");
            Console.ResetColor();
        });

// Simulate multiple requests
for (int i = 1; i <= 10; i++)
{
    try
    {
        await circuitBreakerPolicy.ExecuteAsync(async () =>
        {
            Console.WriteLine($"Request {i}: Calling service...");

            // Simulate random failure
            var rand = new Random();
            if (rand.Next(0, 2) == 0)
                throw new HttpRequestException("Service failed!");

            Console.WriteLine($"Request {i}: Service succeeded!");
            await Task.Delay(200); // Simulate network delay
        });
    }
    catch (BrokenCircuitException)
    {
        Console.WriteLine($"Request {i}: Circuit is open! Skipping request.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Request {i}: Exception: {ex.Message}");
    }

    await Task.Delay(500);
}

Console.WriteLine("Demo finished.");
