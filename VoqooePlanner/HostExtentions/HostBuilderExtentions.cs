using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VoqooePlanner.DbContexts;
using VoqooePlanner.Services;
using VoqooePlanner.Services.Database;
using VoqooePlanner.Stores;
using VoqooePlanner.ViewModels;
using VoqooePlanner.ViewModels.MainViews;

namespace VoqooePlanner.HostExtentions
{
    public static class HostBuilderExtentions
    {
        public static IHostBuilder AddDatabase(this IHostBuilder hostBuilder, string connectionString)
        {
            hostBuilder.ConfigureServices(services =>
            {
                //Database
                services.AddSingleton<IVoqooeDbContextFactory>(new VoqooeDbContextFactory(connectionString));
                services.AddSingleton<IVoqooeDatabaseProvider, VoqooeDatabaseProvider>();
            });

            return hostBuilder;
        }

        public static IHostBuilder AddViewModels(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(services =>
            {
                //VMs
                services.AddTransient(s => CreateVoqooeLisViewModel(s));
                services.AddTransient(s => CreateSettingViewModel(s));
                services.AddTransient<OrganicCheckListViewModel>();
                services.AddSingleton<LoaderViewModel>();
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<NavigationViewModel>();
                //Navigations
                services.AddSingleton<Func<VoqooeListViewModel>>((s) => () => CreateVoqooeLisViewModel(s));
                services.AddSingleton<NavigationService<VoqooeListViewModel>>();
                services.AddSingleton<NavigationService<OrganicCheckListViewModel>>();
                AddViewModelNavigation<OrganicCheckListViewModel>(services);
                services.AddSingleton<Func<SettingsViewModel>>((s) => () => CreateSettingViewModel(s));
                services.AddSingleton<NavigationService<SettingsViewModel>>();
            });

            return hostBuilder;
        }

        public static IHostBuilder AddStores(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(services =>
            {
                //Stores
                services.AddSingleton<VoqooeDataStore>();
                services.AddSingleton<NavigationStore>();
                services.AddSingleton<JournalWatcherStore>();
                services.AddSingleton<SettingsStore>();
            });

            return hostBuilder;
        }

        public static IHostBuilder AddServices(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(services =>
            {
                //Services
                services.AddSingleton<SystemsUpdateService>();
            });

            return hostBuilder;
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
                services.GetRequiredService<VoqooeDataStore>(),
                services.GetRequiredService<JournalWatcherStore>());
        }
        private static void AddViewModelNavigation<TViewModel>(IServiceCollection services) where TViewModel : ViewModelBase
        {
            services.AddSingleton<Func<TViewModel>>((s) => () => s.GetRequiredService<TViewModel>());
            services.AddSingleton<NavigationService<TViewModel>>();
        }
    }
}
