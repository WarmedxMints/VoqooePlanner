using Microsoft.EntityFrameworkCore;
using ODUtils.Dialogs;
using ODUtils.IO;
using System.Diagnostics;
using System.IO;
using System.Windows;
using VoqooePlanner.DbContexts;
using VoqooePlanner.Models;
using VoqooePlanner.Services;
using VoqooePlanner.Services.Database;
using VoqooePlanner.Stores;

namespace VoqooePlanner.ViewModels.MainViews
{
    public sealed class LoaderViewModel(IVoqooeDatabaseProvider voqooeDatabaseProvider,
                                        SystemsUpdateService systemsUpdateService,
                                        IVoqooeDbContextFactory factory,
                                        LoggerStore loggerStore) : ViewModelBase
    {
        private readonly IVoqooeDatabaseProvider voqooeDatabaseProvider = voqooeDatabaseProvider;
        private readonly SystemsUpdateService systemsUpdateService = systemsUpdateService;
        private readonly IVoqooeDbContextFactory factory = factory;
        private readonly LoggerStore loggerStore = loggerStore;
        private string loadingText = "Loading...";
        public string LoadingText
        {
            get => loadingText;
            set
            {
                loadingText = value;
                OnPropertyChanged(nameof(LoadingText));
            }
        }

        public Action? OnUpdateComplete;
        public MessageBoxResult Result { get; private set; }
        public async Task CheckForUpdates(Window loaderWindow)
        {
            OnUpdateTextChange("Checking For App Updates");
            await Task.Delay(1000);
            var updateInfo = await Json.GetJsonFromUrlAndDeserialise<UpdateInfo>("https://raw.githubusercontent.com", "/WarmedxMints/ODUpdates/main/VoqooePlannerUpdate.json");

            if (updateInfo.Version > App.AppVersion)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var result = ODMessageBox.ShowUpdateWindow(loaderWindow, "Voqooe Planner", $"Version {updateInfo.Version} is available.\nWould you like to download?", updateInfo.PatchNotes);

                    if (result == MessageBoxResult.Yes)
                    {
                        var filename = Path.GetFileName(updateInfo.FileUrl);
                        var tempPath = Path.GetTempPath();
                        var filePath = Path.Combine(tempPath, filename);

                        result = ODMessageBox.ShowDownloadBox(loaderWindow, "Voqooe Planner", $"Downloading {filename}", updateInfo.FileUrl, filePath);

                        if (result == MessageBoxResult.OK)
                        {
                            Process.Start(filePath);
                            Result = MessageBoxResult.Cancel;
                            OnUpdateComplete?.Invoke();
                            Application.Current.Shutdown();
                            return;
                        }

                        result = ODMessageBox.ShowUpdateWindow(loaderWindow, "Voqooe Planner", "Error downlading setup file.\nWould you like to go to the download page?", updateInfo.PatchNotes);
                        if (result == MessageBoxResult.OK)
                        {
                            ODUtils.Helpers.OperatingSystem.OpenUrl(updateInfo.Url);
                        }
                    }
                });
            }

            try
            {
                OnUpdateTextChange("Migrating Database");
                await Task.Delay(1000);

                using var dbContext = factory.CreateDbContext();
                await dbContext.Database.MigrateAsync();

                await systemsUpdateService.UpdateDataBaseSystems(OnUpdateTextChange);
            }
            catch (Exception ex)
            {
                loggerStore.LogError("Update Checker", ex);
            }
            await Task.Delay(1000);

            OnUpdateTextChange("Loading Application");

            await Task.Delay(1000);

            OnUpdateComplete?.Invoke();
        }

        private void OnUpdateTextChange(string obj)
        {
            Application.Current.Dispatcher.Invoke(() => LoadingText = obj);
        }
    }
}
