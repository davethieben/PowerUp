using System.Threading;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PowerUp.Helpers;
using PowerUp.Lego;
using SharpBrick.PoweredUp;

namespace PowerUp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IHost _poweredUpHost;
        private CancellationTokenSource _cancelSource;
        private readonly IHostApplicationLifetime _lifetime;

        public MainWindow(IHostApplicationLifetime lifetime)
        {
            _lifetime = lifetime;

            InitializeComponent();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (_poweredUpHost != null)
                return;

            _cancelSource?.Dispose();
            _cancelSource = new CancellationTokenSource();
            _lifetime.ApplicationStopping.Register(() =>
            {
                _cancelSource.Cancel();
            });

            _poweredUpHost = new HostBuilder()
                .ConfigureServices((context, services) => services
                    .AddLogging(logging =>
                    {
                        logging.SetMinimumLevel(LogLevel.Trace);
                        logging.AddDebug();
                        logging.AddTextBox(OutputTextBox, _cancelSource.Token);

                        logging.AddFilter("Microsoft", LogLevel.Warning);
                        logging.AddFilter("SharpBrick.PoweredUp.Bluetooth.BluetoothKernel", LogLevel.Information);

                    })

                    .AddPoweredUp()
                    .AddWinRTBluetooth() // using WinRT Bluetooth on Windows (separate NuGet SharpBrick.PoweredUp.WinRT; others are available)

                    .AddHostedService<StartupTask>()
                )
                .Build();

            await _poweredUpHost.StartAsync(_cancelSource.Token);

            _cancelSource.Token.Register(() =>
            {
                if (_poweredUpHost != null)
                {
                    _poweredUpHost.StopAsync().ConfigureAwait(false);
                    _poweredUpHost.Dispose();
                    _poweredUpHost = null;
                }
            }, useSynchronizationContext: true);

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cancelSource != null)
            {
                _cancelSource.Cancel();
            }
        }
		
    }
}
