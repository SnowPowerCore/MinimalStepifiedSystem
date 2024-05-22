using AspectInjector.Broker;
using Microsoft.Extensions.DependencyInjection;
using MinimalStepifiedSystem.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace MinimalStepifiedSystem.Attributes;

[Aspect(Scope.Global)]
[Injection(typeof(ServiceProviderSupplierAttribute))]
[Mixin(typeof(IServiceProviderSupplier))]
[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
public class ServiceProviderSupplierAttribute : Attribute, IServiceProviderSupplier
{
    private FieldInfo? _serviceProviderDisposedField;

    [NotNull]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public IServiceProvider ServiceProvider { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [Advice(Kind.After)]
    public void Setup([Argument(Source.Arguments)] object[] args)
    {
        var serviceProvider = (IServiceProvider)args.FirstOrDefault(x => x is IServiceProvider);

        if (ServiceProvider is default(IServiceProvider))
        {
            SetServiceProvider(serviceProvider);
            _serviceProviderDisposedField = ServiceProvider!.GetType().GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance);
            return;
        }

        if ((bool)(_serviceProviderDisposedField?.GetValue(ServiceProvider) ?? true))
        {
            SetServiceProvider(serviceProvider);
        }
    }

    private void SetServiceProvider(IServiceProvider serviceProvider)
    {
        Console.WriteLine("Setting the global service scope factory.");
        ServiceProvider = serviceProvider.CreateScope().ServiceProvider;
    }
}