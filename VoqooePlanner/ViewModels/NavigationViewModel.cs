﻿using System.Windows.Input;
using VoqooePlanner.Commands;
using VoqooePlanner.Services;

namespace VoqooePlanner.ViewModels
{
    public sealed class NavigationViewModel(NavigationService<VoqooeListViewModel> routeListCommand,
                                                   NavigationService<SettingsViewModel> settingViewCommand)
    {
        public ICommand VoqooeListCommand { get; } = new NavigateCommand<VoqooeListViewModel>(routeListCommand);
        public ICommand SettingViewCommand { get; } = new NavigateCommand<SettingsViewModel>(settingViewCommand);
    }
}