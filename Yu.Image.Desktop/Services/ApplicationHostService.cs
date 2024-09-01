using Microsoft.Extensions.Hosting;

using Yu.Image.Desktop.Views.Windows;

namespace Yu.Image.Desktop.Services;

public class ApplicationHostService : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        MainWindow mainWindow = App.GetService<MainWindow>();
        mainWindow.Show();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}