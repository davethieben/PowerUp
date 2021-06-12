using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PowerUp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost _host;
        private MainWindow _mainWindow;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<Application>(this);
                    new Startup().ConfigureServices(services);
                })
                .Build();

        }

        protected void OnStartup(object sender, StartupEventArgs e)
        {
            _mainWindow = _host.Services.GetRequiredService<MainWindow>();
            _mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mainWindow?.Dispose();
            _host?.Dispose();

            base.OnExit(e);
        }

    }
}
