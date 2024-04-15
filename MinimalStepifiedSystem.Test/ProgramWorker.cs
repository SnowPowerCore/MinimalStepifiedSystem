using Microsoft.Extensions.Hosting;
using MinimalStepifiedSystem.Test.Interfaces;

namespace MinimalStepifiedSystem.Test;

public class ProgramWorker(IExampleService consumer) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await consumer.ExecuteAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}