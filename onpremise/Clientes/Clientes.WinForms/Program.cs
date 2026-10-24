using Clientes.WinForms.Forms;
using Clientes.WinForms.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Clientes.WinForms;

internal static class Program
{
    // URL base de la API. Ajustar si cambia el puerto en launchSettings.json de la API.
    private const string API_BASE_URL = "http://localhost:5160/";

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var services = new ServiceCollection();

        services.AddHttpClient<ClienteApiService>(client =>
        {
            client.BaseAddress = new Uri(API_BASE_URL);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddTransient<ClientesForm>();

        using var provider = services.BuildServiceProvider();
        var form = provider.GetRequiredService<ClientesForm>();
        Application.Run(form);
    }
}
