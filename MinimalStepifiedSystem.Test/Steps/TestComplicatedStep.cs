﻿using Microsoft.ApplicationInsights;
using MinimalStepifiedSystem.Interfaces;
using MinimalStepifiedSystem.Test.Context;
using MinimalStepifiedSystem.Test.Delegates;

namespace MinimalStepifiedSystem.Test.Steps;

public class TestComplicatedStep(TelemetryClient telemetryClient) : IStep<TestDelegate, TestContext>
{
    public async Task InvokeAsync(TestContext context, TestDelegate next, CancellationToken token = default)
    {
        Console.WriteLine($"Stepified system continues to process");
        await Task.Delay(3000, token);
        telemetryClient.TrackEvent("Complicated step has been executed");
        await next(context, token);
    }
}