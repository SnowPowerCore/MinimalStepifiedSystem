using MinimalStepifiedSystem.Attributes;
using MinimalStepifiedSystem.Test.Context;
using MinimalStepifiedSystem.Test.Delegates;
using MinimalStepifiedSystem.Test.Interfaces;
using MinimalStepifiedSystem.Test.Steps;

namespace MinimalStepifiedSystem.Test;

public class ExampleService : IExampleService
{
    [StepifiedProcess(Steps = [
        typeof(TestInitStep),
        typeof(TestComplicatedStep),
        typeof(TestFinishStep)
    ])]
    protected TestDelegate Execute { get; }

    [StepifiedProcess(Steps = [
        typeof(TestInitStep),
        typeof(TestComplicatedStep),
        typeof(TestFinishStep)
    ])]
    protected TestDelegate AnotherExecute { get; }

    [ServiceProviderSupplier]
    public ExampleService(IServiceProvider _) { }

    public async Task ExecuteAsync()
    {
        using var cts = new CancellationTokenSource();
        await Execute.Invoke(new TestContext { InitData = "Welcome!" }, cts.Token);
    }

    public async Task AnotherExecuteAsync()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        await AnotherExecute.Invoke(new TestContext(), cts.Token);
    }
}