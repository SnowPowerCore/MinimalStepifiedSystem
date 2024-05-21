using Microsoft.Extensions.DependencyInjection;
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

    [ServiceScopeFactorySupplier]
    public ExampleService(IServiceScopeFactory _) { }

    public async Task ExecuteAsync()
    {
        await Execute.Invoke(new TestContext { InitData = "Welcome!" });
    }

    public async Task AnotherExecuteAsync()
    {
        await AnotherExecute.Invoke(new TestContext());
    }
}