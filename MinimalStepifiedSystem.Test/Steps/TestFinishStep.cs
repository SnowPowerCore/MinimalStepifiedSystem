using MinimalStepifiedSystem.Interfaces;
using MinimalStepifiedSystem.Test.Context;
using MinimalStepifiedSystem.Test.Delegates;

namespace MinimalStepifiedSystem.Test.Steps;

public class TestFinishStep : IStep<TestDelegate, TestContext>
{
    public Task InvokeAsync(TestContext context, TestDelegate next)
    {
        var data = context.GetFromData<string>("Message");
        Console.WriteLine($"Stepified system has successfully ran: {data}");
        return next(context);
    }
}