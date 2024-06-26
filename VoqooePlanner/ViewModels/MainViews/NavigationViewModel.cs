﻿using System.Windows.Input;
using VoqooePlanner.Commands;
using VoqooePlanner.Services;

namespace VoqooePlanner.ViewModels.MainViews
{
    public sealed class NavigationViewModel(NavigationService<VoqooeListViewModel> routeListCommand,
                                                   NavigationService<SettingsViewModel> settingViewCommand,
                                                   NavigationService<OrganicCheckListViewModel> organicViewCommand,
                                                   NavigationService<CartoDataViewModel> cartoViewCommand)
    {
        public ICommand VoqooeListCommand { get; } = new NavigateCommand<VoqooeListViewModel>(routeListCommand);
        public ICommand SettingViewCommand { get; } = new NavigateCommand<SettingsViewModel>(settingViewCommand);
        public ICommand OrganicViewCommand { get; } = new NavigateCommand<OrganicCheckListViewModel>(organicViewCommand);
        public ICommand CartoViewCommand { get; } = new NavigateCommand<CartoDataViewModel>(cartoViewCommand);
    }
}