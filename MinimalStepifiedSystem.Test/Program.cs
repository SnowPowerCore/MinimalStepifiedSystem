using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MinimalStepifiedSystem.Core.Extensions;
using MinimalStepifiedSystem.Test;
using MinimalStepifiedSystem.Test.Interfaces;
using MinimalStepifiedSystem.Test.Steps;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTransient<IExampleService, ExampleService>();
builder.Services.AddTransient<TelemetryClient>();
builder.Services.AddSingleton<TestInitStep>();
builder.Services.AddSingleton<TestComplicatedStep>();
builder.Services.AddSingleton<TestFinishStep>();
builder.Services.AddHostedService<ProgramWorker>();

var host = builder.Build();
host.UseStepifiedSystem();
await host.RunAsync();