using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using ODUtils.Dialogs;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using VoqooePlanner.HostExtentions;
using VoqooePlanner.Services;
using VoqooePlanner.Stores;
using VoqooePlanner.ViewModels.MainViews;
using VoqooePlanner.Windows;

namespace VoqooePlanner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly Version AppVersion = new(1, 1, 3, 1);
#if INSTALL
        public readonly static string BaseDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "VoqooePlanner");
#else
        public readonly static string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
#endif
        private const string database = "VoqooePlanner.db";

        private readonly string connectionString = $"DataSource={System.IO.Path.Combine(BaseDirectory, database)}";


        private readonly string logFile = Path.Combine(BaseDirectory, "VPLog.txt");
        private readonly IHost _host;
        public App()
        {
            LogManager.Setup().LoadConfiguration(builder =>
            {
                builder.ForLogger().FilterMinLevel(LogLevel.Debug).WriteToFile(logFile);
                builder.ForLogger().FilterMinLevel(LogLevel.Trace).WriteToFile(logFile);
                builder.ForLogger().FilterMinLevel(LogLevel.Info).WriteToFile(logFile);
                builder.ForLogger().FilterMinLevel(LogLevel.Warn).WriteToFile(logFile);
                builder.ForLogger().FilterMinLevel(LogLevel.Error).WriteToFile(logFile);
                builder.ForLogger().FilterMinLevel(LogLevel.Fatal).WriteToFile(logFile);
            });

            _host = Host.CreateDefaultBuilder()
                .AddDatabase(connectionString)
                .AddViewModels()
                .AddStores()
                .AddServices()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient<EdsmApiService>((httpclient) =>
                    {
                        httpclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        httpclient.BaseAddress = new Uri("https://www.edsm.net/api-system-v1/");
                    })
                    .ConfigurePrimaryHttpMessageHandler(() =>
                    {
                        return new SocketsHttpHandler
                        {
                            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
                        };
                    });
                    services.AddSingleton<LoggerStore>();
                    //Windows
                    services.AddTransient<LoaderWindow>();
                    services.AddSingleton(s => new MainWindow()
                    {
                        DataContext = s.GetRequiredService<MainViewModel>()
                    });
                })
                .Build();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Process proc = Process.GetCurrentProcess();
            int count = Process.GetProcesses().Where(p =>
                p.ProcessName == proc.ProcessName).Count();

            if (count > 1)
            {
                ODMessageBox.ShowNoOwner("Voqooe Planner", "Voqooe Planner is already running\nApplication will now close");
                App.Current.Shutdown();
                return;
            }

            if (System.IO.Directory.Exists(BaseDirectory) == false)
            {
                System.IO.Directory.CreateDirectory(BaseDirectory);
            }
            _host.Start();

#if DEBUG
            var createDb = !File.Exists(Path.Combine(BaseDirectory, database));

            if (!createDb)
            {
#endif
                //Disable shutdown when the dialog closes
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
                var updateWindow = _host.Services.GetRequiredService<LoaderWindow>();
                var model = _host.Services.GetRequiredService<LoaderViewModel>();
                updateWindow.DataContext = model;
                _ = updateWindow.ShowDialog();

                if(model.Result == MessageBoxResult.Cancel)
                {
                    return;
                }

                ShutdownMode = ShutdownMode.OnMainWindowClose;
#if DEBUG
            }
#endif
            var settingsStore = _host.Services.GetRequiredService<SettingsStore>();
            settingsStore.LoadSettings();

            var navigationService = _host.Services.GetRequiredService<NavigationService<VoqooeListViewModel>>();
            navigationService.Navigate();

            MainWindow = _host.Services.GetRequiredService<MainWindow>();
            MainWindow.Show();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            var settings = _host.Services.GetRequiredService<SettingsStore>();
            settings.SaveSettings();
            _host.Dispose();
            base.OnExit(e);
        }
    }
}