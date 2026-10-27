using Microsoft.Extensions.DependencyInjection;
using OnPremise.WinForms.Services;

namespace OnPremise.WinForms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var services = new ServiceCollection();

        // URL base de la API (ajustable). Se mantiene el puerto HTTP de desarrollo.
        var apiBaseUrl = Environment.GetEnvironmentVariable("CLIENTES_API_URL") ?? "http://localhost:5104/";

        services.AddHttpClient<ClienteApiService>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddTransient<ClienteForm>();

        using var provider = services.BuildServiceProvider();
        var form = provider.GetRequiredService<ClienteForm>();
        Application.Run(form);
    }
}
