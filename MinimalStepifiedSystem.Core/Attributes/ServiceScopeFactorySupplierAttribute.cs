using System.Diagnostics.CodeAnalysis;
using AspectInjector.Broker;
using Microsoft.Extensions.DependencyInjection;
using MinimalStepifiedSystem.Interfaces;

namespace MinimalStepifiedSystem.Attributes;

[Aspect(Scope.Global)]
[Injection(typeof(ServiceScopeFactorySupplierAttribute))]
[Mixin(typeof(IServiceScopeFactorySupplier))]
[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
public class ServiceScopeFactorySupplierAttribute : Attribute, IServiceScopeFactorySupplier
{
    [NotNull]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public IServiceScopeFactory ServiceScopeFactory { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [Advice(Kind.After)]
    public void Setup([Argument(Source.Arguments)] object[] args)
    {
        Console.WriteLine("Setting the global service scope factory.");
        ServiceScopeFactory = (IServiceScopeFactory)args.FirstOrDefault(x => x is IServiceScopeFactory)!;
    }
}