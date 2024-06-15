using ODUtils.Helpers;
using ODUtils.IO;
using VoqooePlanner.DTOs;
using VoqooePlanner.Services.Database;

namespace VoqooePlanner.Services
{
    public sealed class SystemsUpdateService(IVoqooeDatabaseProvider voqooeDatabase)
    {
        private readonly IVoqooeDatabaseProvider voqooeDatabase = voqooeDatabase;
        private SystemTimer? updateTimer;

        public void SetUpdateTimer(Action<string> updateTextMethod)
        {
            var date = DateTime.Now;
            var updateTime = new DateTime(date.Year, date.Month, date.Day, date.Hour, 10, 0);

            var timeToGo = updateTime.Subtract(date);

            if (timeToGo < TimeSpan.FromMinutes(10))
            {
                timeToGo += TimeSpan.FromHours(1);
            }

            updateTimer ??= new();
            updateTimer.SetUpTimerFromTimeSpan(timeToGo, () => UpdateSystems(updateTextMethod), "Systems Update Service");
        }

        private async Task UpdateSystems(Action<string> updateTextMethod)
        {
            updateTextMethod.Invoke("AutoUpdateStart");
            await UpdateDataBaseSystems(updateTextMethod);
            updateTextMethod.Invoke("AutoUpdateEnd");
            SetUpdateTimer(updateTextMethod);
        }

        public async Task UpdateDataBaseSystems(Action<string> updateTextMethod)
        {
            updateTextMethod.Invoke("Checking For Systems Update");
            await Task.Delay(1000);

            var systems = await Json.GetJsonFromUrlAndDeserialise<IEnumerable<VoqooeSystemDTO>>("https://raw.githubusercontent.com", "/WarmedxMints/ODUpdates/main/VoqooeSystemsDTO.Json");

            if (systems != null && systems.Any())
            {
                var updatedCount = await voqooeDatabase.UpdateVoqooeSystems(systems);

                if (updatedCount > 0)
                {
                    var updateText = updatedCount > 1 ? "Systems..." : "System...";
                    updateTextMethod.Invoke($"Updated {updatedCount:N0} {updateText}");
                    await Task.Delay(1000);
                    return;
                }

                updateTextMethod.Invoke("No Update Required");
                await Task.Delay(1000);
                return;
            }

            updateTextMethod.Invoke($"Error Obtaining Lastest Systems File");
            await Task.Delay(1000);
        }
    }
}
