using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnPremise.Domain.Interfaces;
using OnPremise.Infrastructure.Persistence;

namespace OnPremise.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Kiro")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'Kiro'.");

        services.AddScoped<IClienteRepository>(_ => new ClienteRepository(connectionString));
        return services;
    }
}
