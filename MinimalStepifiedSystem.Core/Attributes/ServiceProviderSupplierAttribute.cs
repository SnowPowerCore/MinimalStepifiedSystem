using AspectInjector.Broker;
using Microsoft.Extensions.DependencyInjection;
using MinimalStepifiedSystem.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace MinimalStepifiedSystem.Attributes;

[Aspect(Scope.Global)]
[Injection(typeof(ServiceProviderSupplierAttribute))]
[Mixin(typeof(IServiceProviderSupplier))]
[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
public class ServiceProviderSupplierAttribute : Attribute, IServiceProviderSupplier
{
    private const string DisposedPropertyName = "Disposed";
    private const string GlobalScopeMessage = "Setting the global service scope factory.";
    private const string InstanceIdentifier = "i";

    private Func<object, bool>? _getIsDisposed;

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
            _getIsDisposed = GenerateGetterLambda(ServiceProvider!.GetType().GetProperty(DisposedPropertyName, BindingFlags.NonPublic | BindingFlags.Instance));
            return;
        }

        if (_getIsDisposed!(ServiceProvider))
        {
            SetServiceProvider(serviceProvider);
        }
    }

    private void SetServiceProvider(IServiceProvider serviceProvider)
    {
        Console.WriteLine(GlobalScopeMessage);
        ServiceProvider = serviceProvider.CreateScope().ServiceProvider;
    }

    private static Func<object, bool> GenerateGetterLambda(PropertyInfo property)
    {
        // Define our instance parameter, which will be the input of the Func
        var objParameterExpr = Expression.Parameter(typeof(object), InstanceIdentifier);
        // 1. Cast the instance to the correct type
        var instanceExpr = Expression.TypeAs(objParameterExpr, property.DeclaringType);
        // 2. Call the getter and retrieve the value of the property
        var propertyExpr = Expression.Property(instanceExpr, property);
        // Create a lambda expression of the latest call & compile it
        return Expression.Lambda<Func<object, bool>>(propertyExpr, objParameterExpr).Compile();
    }
}