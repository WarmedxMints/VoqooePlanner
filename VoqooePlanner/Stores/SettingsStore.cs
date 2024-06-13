using System.Windows;
using VoqooePlanner.DTOs;
using VoqooePlanner.Models;
using VoqooePlanner.Services.Database;
using VoqooePlanner.ViewModels.ModelViews;

namespace VoqooePlanner.Stores
{
    public sealed class SettingsStore(IVoqooeDatabaseProvider voqooeDatabase)
    {
        private readonly IVoqooeDatabaseProvider voqooeDatabase = voqooeDatabase;

        public int SelectedCommanderID { get; set; } = 0;
        public NearBySystemsOptions NearBySystemsOptions { get; set; } = NearBySystemsOptions.ExcludeVisited | NearBySystemsOptions.ExcludeUserVisited | NearBySystemsOptions.IncludeUnvisitedELWs | NearBySystemsOptions.IncludeUnvisitedValue;
        public bool AutoSelectNextSystemInRoute { get; set; } = true;
        public bool AutoCopyNextSystemToClipboard { get; set; } = true;
        public bool ContinuousRoute { get; set; } = false;
        public List<int> StarClassFilter { get; set; } = [-1];
        public WindowPositionViewModel WindowPosition { get; set; } = new();
        public ExoBiologyViewState BiologyViewState { get; set; } = ExoBiologyViewState.CheckList;

        public void LoadSettings()
        {
            var settings = voqooeDatabase.GetAllSettings();

            if(settings != null && settings.Any()) 
            { 
                SelectedCommanderID             = SettingsDTO.SettingsDtoToInt(settings.GetSettingDTO(nameof(SelectedCommanderID)));
                NearBySystemsOptions            = SettingsDTO.SettingsDtoToEnum(settings.GetSettingDTO(nameof(NearBySystemsOptions)), NearBySystemsOptions);
                AutoSelectNextSystemInRoute     = SettingsDTO.SettingsDtoToBool(settings.GetSettingDTO(nameof(AutoSelectNextSystemInRoute)), true);
                AutoCopyNextSystemToClipboard   = SettingsDTO.SettingsDtoToBool(settings.GetSettingDTO(nameof(AutoCopyNextSystemToClipboard)), true);
                ContinuousRoute                 = SettingsDTO.SettingsDtoToBool(settings.GetSettingDTO(nameof(ContinuousRoute)), false);
                StarClassFilter                 = SettingsDTO.SettingDtoToJson(settings.GetSettingDTO(nameof(StarClassFilter)), StarClassFilter);
                WindowPosition                  = SettingsDTO.SettingDtoToJson(settings.GetSettingDTO(nameof(WindowPosition)), WindowPosition);
                BiologyViewState                = SettingsDTO.SettingDtoToEnum(settings.GetSettingDTO(nameof(BiologyViewState)), BiologyViewState);
            }

            if(WindowPosition.IsZero)
            {
                ResetWindowPosition();
            }
        }

        public void SaveSettings()
        {
            var settings = new List<SettingsDTO>
            {
                SettingsDTO.IntToSettingsDTO(nameof(SelectedCommanderID), SelectedCommanderID),
                SettingsDTO.EnumToSettingsDto(nameof(NearBySystemsOptions), NearBySystemsOptions),
                SettingsDTO.BoolToSettingsDTO(nameof(AutoSelectNextSystemInRoute), AutoSelectNextSystemInRoute),
                SettingsDTO.BoolToSettingsDTO(nameof(AutoCopyNextSystemToClipboard), AutoCopyNextSystemToClipboard),
                SettingsDTO.BoolToSettingsDTO(nameof(ContinuousRoute), ContinuousRoute),
                SettingsDTO.ObjectToJsonStringDto(nameof(StarClassFilter), StarClassFilter),
                SettingsDTO.ObjectToJsonStringDto(nameof(WindowPosition), WindowPosition),
                SettingsDTO.EnumToSettingsDto(nameof(BiologyViewState), BiologyViewState),
            };

            voqooeDatabase.AddSettings(settings);
        }

        public void ResetWindowPosition()
        {
            ResetWindowPositionActual(WindowPosition);
        }

        private static void ResetWindowPositionActual(WindowPositionViewModel windowPosition)
        {
            double windowWidth = 1650;
            double windowHeight = 900;

            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            var left = (screenWidth / 2) - (windowWidth / 2);
            var top = (screenHeight / 2) - (windowHeight / 2);

            if (windowHeight > SystemParameters.VirtualScreenHeight)
            {
                windowHeight = SystemParameters.VirtualScreenHeight;
            }

            if (windowWidth > SystemParameters.VirtualScreenWidth)
            {
                windowWidth = SystemParameters.VirtualScreenWidth;
            }

            windowPosition.Top = top;
            windowPosition.Left = left;
            windowPosition.Width = windowWidth;
            windowPosition.Height = windowHeight;
            windowPosition.State = WindowState.Normal;
        }
    }
}