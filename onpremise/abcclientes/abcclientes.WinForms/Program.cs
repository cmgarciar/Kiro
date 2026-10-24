using abcclientes.WinForms.Forms;
using abcclientes.WinForms.Services;
using Microsoft.Extensions.DependencyInjection;

namespace abcclientes.WinForms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var services = new ServiceCollection();
        services.AddHttpClient<CustomerApiService>();
        services.AddSingleton<CustomerForm>();

        var provider = services.BuildServiceProvider();
        System.Windows.Forms.Application.Run(provider.GetRequiredService<CustomerForm>());
    }
}
