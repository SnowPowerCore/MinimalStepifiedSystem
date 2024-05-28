using MinimalStepifiedSystem.Interfaces;
using MinimalStepifiedSystem.Test.Context;
using MinimalStepifiedSystem.Test.Delegates;

namespace MinimalStepifiedSystem.Test.Steps;

public class TestInitStep : IStep<TestDelegate, TestContext>
{
    public async Task InvokeAsync(TestContext context, TestDelegate next, CancellationToken token = default)
    {
        try
        {
            Console.WriteLine($"Stepified system has been initialized: {context.InitData}");
            context.SetDataWith("Message", "Very Important Message");
            await next(context, token);
        }
        catch (TaskCanceledException e)
        {
            Console.WriteLine($"Handled a cancellation for {context.GetType().FullName}: {e.Message}");
        }
    }
}