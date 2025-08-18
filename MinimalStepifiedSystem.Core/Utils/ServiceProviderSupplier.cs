using Microsoft.Extensions.DependencyInjection;

namespace MinimalStepifiedSystem.Utils;

public class ServiceProviderSupplier
{
    private static readonly Lazy<ServiceProviderSupplier> _instance =
        new(() => new ServiceProviderSupplier());

    private IServiceScopeFactory? _scopeFactory;

    public static ServiceProviderSupplier Instance => _instance.Value;

    private ServiceProviderSupplier() { }

    /// <summary>
    /// Safe to call on app restarts.
    /// </summary>
    public void Setup(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));

        _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
    }

    /// <summary>
    /// Creates a fresh scope for isolated, short-lived resolutions.
    /// </summary>
    public IServiceScope CreateScope()
    {
        if (_scopeFactory == null)
            throw new InvalidOperationException("ServiceProviderSupplier not initialized. Call Setup() first.");

        return _scopeFactory.CreateScope();
    }
}