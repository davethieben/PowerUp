using System;
using System.Threading;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PowerUp.Helpers;
using PowerUp.Lego;
using PowerUp.Services;
using SharpBrick.PoweredUp;

namespace PowerUp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private IHost? _poweredUpHost;
        private CancellationTokenSource? _cancelSource;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ILogger<MainWindow> _logger;

        public MainWindow(IHostApplicationLifetime lifetime, ILogger<MainWindow> logger)
        {
            _lifetime = lifetime;
            _logger = logger;

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

                        //logging.AddFilter("Microsoft", LogLevel.Warning);
                        logging.AddFilter("SharpBrick.PoweredUp.Bluetooth.BluetoothKernel", LogLevel.Information);

                    })

                    .AddPoweredUp()
                    .AddWinRTBluetooth() // using WinRT Bluetooth on Windows (separate NuGet SharpBrick.PoweredUp.WinRT; others are available)

                    .AddHostedService<StartupTask>()
                    .AddSingleton<MessageHub>()

                )
                .Build();

            var messageHub = _poweredUpHost.Services.GetRequiredService<MessageHub>();
            messageHub.Subscribe("MotorAPosition", data =>
            {
                _motorAPositionTextBox.Dispatch(textbox =>
                {
                    textbox.Text = data?.ToString();
                });
            });

            await _poweredUpHost.StartAsync(_cancelSource.Token);

            _cancelSource.Token.Register(() =>
            {
                _logger.LogDebug("Cancel signalled, shutting down hosting...");

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
            _logger.LogInformation("Attempting to Cancel connection...");
            if (_cancelSource != null)
            {
                _cancelSource.Cancel();
            }
        }

        public void Dispose()
        {
            _poweredUpHost?.Dispose();
        }
    }
}
