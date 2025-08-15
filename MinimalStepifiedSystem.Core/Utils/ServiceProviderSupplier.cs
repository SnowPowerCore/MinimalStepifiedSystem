using Microsoft.Extensions.DependencyInjection;

namespace MinimalStepifiedSystem.Core.Utils;

public class ServiceProviderSupplier
{
    private const string GlobalScopeMessage = "Setting the global service scope factory.";

    private static readonly Lazy<ServiceProviderSupplier> _instance = new(() => new ServiceProviderSupplier());
    private IServiceProvider? _serviceProvider;
    private IServiceScopeFactory? _serviceScopeFactory;

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
            return;
        }
    }

    public IServiceProvider? GetServiceProvider() => _serviceProvider;

    private void SetServiceProvider(IServiceScopeFactory serviceScopeFactory)
    {
        Console.WriteLine(GlobalScopeMessage);
        _serviceProvider = serviceScopeFactory.CreateScope().ServiceProvider;
    }
}