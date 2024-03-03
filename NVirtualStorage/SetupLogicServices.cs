using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NVirtualStorage;

public static class SetupVirtualStorage
{
    public static void AddVirtualStorage(this IServiceCollection services,
        IConfiguration configurationBuilder)
    {
        services.AddScoped<VirtualStorage>();

        var configurationSection = configurationBuilder.GetSection("VirtualStorage");
        services.Configure<VirtualStorageRootConfig>(configurationSection);
    }
}