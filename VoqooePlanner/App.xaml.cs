using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using ODUtils.Dialogs;
using System.Diagnostics;
using System.IO;
using System.Windows;
using VoqooePlanner.DbContexts;
using VoqooePlanner.Services;
using VoqooePlanner.Services.Database;
using VoqooePlanner.Stores;
using VoqooePlanner.ViewModels;
using VoqooePlanner.Windows;

namespace VoqooePlanner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly Version AppVersion = new(1, 0);
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
                .ConfigureServices((hostContext, services) =>
                {
                    //Database
                    services.AddSingleton<IVoqooeDbContextFactory>(new VoqooeDbContextFactory(connectionString));
                    services.AddSingleton<IVoqooeDatabaseProvider, VoqooeDatabaseProvider>();
                    //VMs
                    services.AddTransient(s => CreateVoqooeLisViewModel(s));
                    services.AddTransient(s => CreateSettingViewModel(s));
                    services.AddSingleton<LoaderViewModel>();
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<NavigationViewModel>();
                    //Navigations
                    services.AddSingleton<Func<VoqooeListViewModel>>((s) => () => CreateVoqooeLisViewModel(s));
                    services.AddSingleton<NavigationService<VoqooeListViewModel>>();
                    services.AddSingleton<Func<SettingsViewModel>>((s) => () => CreateSettingViewModel(s));
                    //Services
                    services.AddSingleton<NavigationService<SettingsViewModel>>();
                    services.AddSingleton<SystemsUpdateService>();
                    //Stores
                    services.AddSingleton<VoqooeDataStore>();
                    services.AddSingleton<NavigationStore>();
                    services.AddSingleton<JournalWatcherStore>();
                    services.AddSingleton<SettingsStore>();
                    //Windows
                    services.AddSingleton<LoaderWindow>();
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
            var createDb = false;
            if (File.Exists(Path.Combine(BaseDirectory, database)) == false)
            {
                createDb = true;
            }
#endif
            var factory = _host.Services.GetRequiredService<IVoqooeDbContextFactory>();
            using (var dbContext = factory.CreateDbContext())
            {
                dbContext.Database.Migrate();
            }

#if DEBUG
            if (createDb)
            {
#endif
            //Disable shutdown when the dialog closes
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var updates = _host.Services.GetRequiredService<LoaderWindow>();
            updates.DataContext = _host.Services.GetRequiredService<LoaderViewModel>();
            updates.ShowDialog();
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
        private static VoqooeListViewModel CreateVoqooeLisViewModel(IServiceProvider services)
        {
            return VoqooeListViewModel.CreateModel(services.GetRequiredService<VoqooeDataStore>(),
                services.GetRequiredService<IVoqooeDatabaseProvider>(),
                services.GetRequiredService<SettingsStore>(),
                services.GetRequiredService<JournalWatcherStore>());
        }

        private static SettingsViewModel CreateSettingViewModel(IServiceProvider services)
        {
            return SettingsViewModel.CreateViewModel(services.GetRequiredService<SettingsStore>(),
                services.GetRequiredService<IVoqooeDatabaseProvider>(),
                services.GetRequiredService<VoqooeDataStore>());
        }
        private static void AddViewModelNavigation<TViewModel>(IServiceCollection services) where TViewModel : ViewModelBase
        {
            services.AddSingleton<Func<TViewModel>>((s) => () => s.GetRequiredService<TViewModel>());
            services.AddSingleton<NavigationService<TViewModel>>();
        }
    }
}