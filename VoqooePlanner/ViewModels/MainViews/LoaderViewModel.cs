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

namespace VoqooePlanner.ViewModels.MainViews
{
    public sealed class LoaderViewModel(IVoqooeDatabaseProvider voqooeDatabaseProvider, SystemsUpdateService systemsUpdateService, IVoqooeDbContextFactory factory) : ViewModelBase
    {
        private readonly IVoqooeDatabaseProvider voqooeDatabaseProvider = voqooeDatabaseProvider;
        private readonly SystemsUpdateService systemsUpdateService = systemsUpdateService;
        private readonly IVoqooeDbContextFactory factory = factory;
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
            await Task.Delay(1000);

            LoadingText = "Migrating Database";

            using (var dbContext = factory.CreateDbContext())
            {
                await dbContext.Database.MigrateAsync();
            }

            await Task.Delay(1000);

            LoadingText = "Checking For App Updates";

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

            await Task.Delay(1000);

            await systemsUpdateService.UpdateDataBaseSystems(OnUpdateTextChange);

            await Task.Delay(1000);

            LoadingText = "Loading Application";

            await Task.Delay(1000);

            OnUpdateComplete?.Invoke();
        }

        private void OnUpdateTextChange(string obj)
        {
            LoadingText = obj;
        }
    }
}
