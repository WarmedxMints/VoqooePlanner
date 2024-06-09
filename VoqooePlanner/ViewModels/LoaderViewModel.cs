using ODUtils.IO;
using System.Windows;
using VoqooePlanner.DTOs;
using VoqooePlanner.Services.Database;

namespace VoqooePlanner.ViewModels
{
    public sealed class LoaderViewModel(IVoqooeDatabaseProvider voqooeDatabaseProvider) : ViewModelBase
    {
        private readonly IVoqooeDatabaseProvider voqooeDatabaseProvider = voqooeDatabaseProvider;

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

			//TODO Check for app updates!

			await Task.Delay(2000);

			LoadingText = "Checking For Systems Updates";

            var systems = await Json.GetJsonFromUrlAndDeserialise<IEnumerable<VoqooeSystemDTO>>("https://raw.githubusercontent.com", "/WarmedxMints/ODUpdates/main/VoqooeSystemsDTO.Json");

            if (systems != null && systems.Any())
            {
                var ret = await voqooeDatabaseProvider.UpdateVoqooeSystems(systems);

                if (ret > 0)
                {
					var updateText = ret > 1 ? "Systems..." : "System...";
                    LoadingText = $"Updated {ret:N0} {updateText}";
                    await Task.Delay(2000);
                }
            }

            await Task.Delay(2000);

			LoadingText = "Loading Application";

			await Task.Delay(2000); 

			OnUpdateComplete?.Invoke();
		}
	}
}
