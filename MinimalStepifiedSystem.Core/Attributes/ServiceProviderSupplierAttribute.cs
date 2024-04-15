using System.Diagnostics.CodeAnalysis;
using AspectInjector.Broker;
using MinimalStepifiedSystem.Interfaces;

namespace MinimalStepifiedSystem.Attributes;

[Aspect(Scope.Global)]
[Injection(typeof(ServiceProviderSupplierAttribute))]
[Mixin(typeof(IServiceProviderSupplier))]
[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
public class ServiceProviderSupplierAttribute : Attribute, IServiceProviderSupplier
{
    [NotNull]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public IServiceProvider ServiceProvider { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [Advice(Kind.After)]
    public void Setup([Argument(Source.Arguments)] object[] args)
    {
        Console.WriteLine("Setting the global service provider.");
        ServiceProvider = (IServiceProvider)args.FirstOrDefault(x => x is IServiceProvider)!;
    }
}