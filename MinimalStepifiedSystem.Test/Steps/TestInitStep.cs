using MinimalStepifiedSystem.Interfaces;
using MinimalStepifiedSystem.Test.Context;
using MinimalStepifiedSystem.Test.Delegates;

namespace MinimalStepifiedSystem.Test.Steps;

public class TestInitStep : IStep<TestDelegate, TestContext>
{
    public Task InvokeAsync(TestContext context, TestDelegate next)
    {
        Console.WriteLine($"Stepified system has been initialized: {context.InitData}");
        context.SetDataWith("Message", "Very Important Message");
        return next(context);
    }
}