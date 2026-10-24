using Clientes.Application.Interfaces;
using Clientes.Application.Services;
using Clientes.Domain.Interfaces;
using Clientes.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Clientes.Infrastructure;

/// <summary>
/// Registro de dependencias de Infraestructura y Aplicación.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddClientesServices(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<IClienteRepository>(_ => new ClienteRepository(connectionString));
        services.AddScoped<IClienteService, ClienteService>();
        return services;
    }
}
