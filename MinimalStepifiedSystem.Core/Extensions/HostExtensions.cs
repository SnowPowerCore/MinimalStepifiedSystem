using Microsoft.Extensions.Hosting;
using MinimalStepifiedSystem.Core.Utils;

namespace MinimalStepifiedSystem.Core.Extensions;

public static class HostExtensions
{
    public static IHost UseStepifiedSystem(this IHost host)
    {
        ServiceProviderSupplier.Instance!.Setup(host.Services);
        return host;
    }
}