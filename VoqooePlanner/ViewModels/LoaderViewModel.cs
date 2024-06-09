using ODUtils.Dialogs;
using ODUtils.IO;
using System.Windows;
using VoqooePlanner.DTOs;
using VoqooePlanner.Models;
using VoqooePlanner.Services;
using VoqooePlanner.Services.Database;

namespace VoqooePlanner.ViewModels
{
    public sealed class LoaderViewModel(IVoqooeDatabaseProvider voqooeDatabaseProvider, SystemsUpdateService systemsUpdateService) : ViewModelBase
    {
        private readonly IVoqooeDatabaseProvider voqooeDatabaseProvider = voqooeDatabaseProvider;
        private readonly SystemsUpdateService systemsUpdateService = systemsUpdateService;
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

		public async Task CheckForUpdates(Window loaderWindow)
		{
			await Task.Delay(1000);
			LoadingText = "Checking For App Updates";

            var updateInfo = await Json.GetJsonFromUrlAndDeserialise<UpdateInfo>("https://raw.githubusercontent.com", "/WarmedxMints/ODUpdates/main/VoqooePlannerUpdate.json");

            if (updateInfo.Version > App.AppVersion)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var result = ODMessageBox.ShowUpdateWindow(loaderWindow, "Voqooe Planner", $"Version {updateInfo.Version} is available.\nWould you like to go to the download page?", updateInfo.PatchNotes);

                    if (result == MessageBoxResult.Yes)
                    {
                        ODUtils.Helpers.OperatingSystem.OpenUrl(updateInfo.Url);
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
