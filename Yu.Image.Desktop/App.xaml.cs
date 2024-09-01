using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Wpf.Ui;

using Yu.Image.Desktop.Services;
using Yu.Image.Desktop.ViewModels.Windows;
using Yu.Image.Desktop.Views.Windows;

namespace Yu.Image.Desktop;

public partial class App
{
    // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    private static readonly IHost _host = Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration(c =>
        {
            var basePath =
                Path.GetDirectoryName(AppContext.BaseDirectory)
                ?? throw new DirectoryNotFoundException(
                    "Unable to find the base directory of the application."
                );
            _ = c.SetBasePath(basePath);
        })
        .ConfigureServices(
            (context, services) =>
            {
                services.AddHostedService<ApplicationHostService>();

                services.AddSingleton<ISnackbarService, SnackbarService>();

                services.AddTransient<MainWindow>();
                services.AddSingleton<MainWindowViewModel>();

                services.AddSingleton<LabCv2Window>();
                services.AddSingleton<LabCv2WindowViewModel>();
            }
        )
        .Build();

    /// <summary>
    /// 获取注册的服务
    /// </summary>
    /// <typeparam name="T">服务类</typeparam>
    /// <returns>获取服务的实例或者<see langword="null"/></returns>
    public static T GetService<T>() where T : class
    {
        return _host.Services.GetService(typeof(T)) as T ?? throw new NullReferenceException();
    }

    /// <summary>
    /// 软件启动入口
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        _host.StartAsync();
    }

    /// <summary>
    /// 软件退出出口
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void App_OnExit(object sender, ExitEventArgs e)
    {
        _host.StopAsync();
    }

    /// <summary>
    /// 全局异常捕捉
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var ErrorMessage = $"{nameof(App_OnDispatcherUnhandledException)}:{e.Exception.Message}";
        Trace.WriteLine(ErrorMessage);
        MessageBox.Show(ErrorMessage);
    }
}