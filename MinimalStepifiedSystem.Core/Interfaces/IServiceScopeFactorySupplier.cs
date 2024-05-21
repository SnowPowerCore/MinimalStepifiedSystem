using Microsoft.Extensions.DependencyInjection;

namespace MinimalStepifiedSystem.Interfaces;

public interface IServiceScopeFactorySupplier
{
    IServiceScopeFactory ServiceScopeFactory { get; set; }
}