using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MinimalStepifiedSystem.Core.Utils;

public class ServiceProviderSupplier
{
    private const string DisposedPropertyName = "Disposed";
    private const string GlobalScopeMessage = "Setting the global service scope factory.";
    private const string InstanceIdentifier = "i";

    private static readonly Lazy<ServiceProviderSupplier> _instance = new(() => new ServiceProviderSupplier());
    private IServiceProvider? _serviceProvider;
    private IServiceScopeFactory? _serviceScopeFactory;
    private Func<object, bool>? _getIsDisposed;

    public static ServiceProviderSupplier Instance => _instance.Value;

    private ServiceProviderSupplier() { }

    public void Setup(IServiceProvider serviceProvider)
    {
        if (serviceProvider is default(IServiceProvider))
            return;

        _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        if (_serviceProvider is default(IServiceProvider))
        {
            SetServiceProvider(_serviceScopeFactory!);
            _getIsDisposed = GenerateGetterLambda(_serviceProvider!.GetType().GetProperty(DisposedPropertyName, BindingFlags.NonPublic | BindingFlags.Instance)!);
            return;
        }
    }

    public IServiceProvider? GetServiceProvider() => !_getIsDisposed!(_serviceProvider!)
        ? _serviceProvider
        : _serviceProvider = _serviceScopeFactory?.CreateScope().ServiceProvider;

    private void SetServiceProvider(IServiceScopeFactory serviceScopeFactory)
    {
        Console.WriteLine(GlobalScopeMessage);
        _serviceProvider = serviceScopeFactory.CreateScope().ServiceProvider;
    }

    private static Func<object, bool> GenerateGetterLambda(PropertyInfo property)
    {
        // Define our instance parameter, which will be the input of the Func
        var objParameterExpr = Expression.Parameter(typeof(object), InstanceIdentifier);
        // 1. Cast the instance to the correct type
        var instanceExpr = Expression.TypeAs(objParameterExpr, property.DeclaringType!);
        // 2. Call the getter and retrieve the value of the property
        var propertyExpr = Expression.Property(instanceExpr, property);
        // Create a lambda expression of the latest call & compile it
        return Expression.Lambda<Func<object, bool>>(propertyExpr, objParameterExpr).Compile();
    }
}