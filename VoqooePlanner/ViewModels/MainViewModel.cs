using ODUtils.Commands;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using VoqooePlanner.Models;
using VoqooePlanner.Stores;

namespace VoqooePlanner.ViewModels
{
    public sealed class MainViewModel : ViewModelBase
    {
        private readonly NavigationStore navStore;
        private readonly VoqooeDataStore voqooeDataStore;
        private readonly NavigationViewModel navigationViewModel;
        private readonly SettingsStore settingsStore;

        public string Title { get; }
        public SettingsStore SettingsStore { get => settingsStore; }
        public ViewModelBase? CurrentViewModel => navStore.CurrentViewModel;
        public string? CurrentSystem => voqooeDataStore.CurrentSystem?.Name;

        public ObservableCollection <JournalCommaderViewModel> JournalCommaders { get; set; } = [];

        private JournalCommaderViewModel? selectedCommander;
        public JournalCommaderViewModel? SelectedCommander
        {
            get => selectedCommander;
            set
            {
                if(value == selectedCommander) 
                    return;
                selectedCommander = value;
                if(UiEnabled && selectedCommander != null && selectedCommander.Id != settingsStore.SelectedCommanderID) 
                    voqooeDataStore.ChangeCommander(selectedCommander.Id);

                OnPropertyChanged(nameof(SelectedCommander));
            }
        }

        private bool uiEnabled;
        public bool UiEnabled
        {
            get => uiEnabled;
            set
            {
                uiEnabled = value;
                OnPropertyChanged(nameof(UiEnabled));
            }
        }

        public ICommand NavigateToVoqooeList { get; }// => navigationViewModel.VoqooeListCommand;
        public ICommand NavigateToSettingView { get; }//=> navigationViewModel.SettingViewCommand;
        public ICommand ResetWindowPositionCommand { get; }

        public MainViewModel(NavigationStore navStore, VoqooeDataStore voqooeDataStore, NavigationViewModel navigationViewModel, SettingsStore settingsStore)
        {
            this.navStore = navStore;
            this.voqooeDataStore = voqooeDataStore;
            this.navigationViewModel = navigationViewModel;
            this.settingsStore = settingsStore;
            navStore.CurrentViewModelChanged += OnCurrentViewModelChaned;
            voqooeDataStore.OnCurrentSystemChanged += OnCurrentSystemChanged;
            voqooeDataStore.OnCurrentCommanderChanged += OnCommanderChanged;
            voqooeDataStore.OnCommandersUpdated += OnCommandersUpdated;
            voqooeDataStore.ReadyStatusChange += OnReadyStateChange;

            NavigateToVoqooeList = new RelayCommand(OnNavigateToList, (_) => CurrentViewModel is not VoqooeListViewModel && UiEnabled);
            NavigateToSettingView = new RelayCommand(OnNavigateToSettings, (_) => CurrentViewModel is not SettingsViewModel && UiEnabled);
            ResetWindowPositionCommand = new RelayCommand(OnResetWindowPos);

            Title = $"Voqooe Planner v{App.AppVersion.Major}.{App.AppVersion.Minor}";
        }

        private void OnNavigateToSettings(object? obj)
        {
            navigationViewModel.SettingViewCommand.Execute(null);
        }

        private void OnNavigateToList(object? obj)
        {
            navigationViewModel.VoqooeListCommand.Execute(null);
        }

        private void OnResetWindowPos(object? obj)
        {
            SettingsStore.ResetWindowPosition();
        }

        private void OnReadyStateChange(object? sender, bool e)
        {
            UiEnabled = e;
        }

        private void OnCommandersUpdated(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                JournalCommaders.Clear();
                foreach (var journalCommader in voqooeDataStore.JournalCommaders)
                {
                    JournalCommaders.Add(new JournalCommaderViewModel(journalCommader));
                };

                OnPropertyChanged(nameof(JournalCommaders));

                var cmdr = JournalCommaders.FirstOrDefault(x => x.Id == settingsStore.SelectedCommanderID);
                SelectedCommander = cmdr;
            });
        }

        private void OnCommanderChanged(object? sender, JournalCommander e)
        {
            var cmdr = JournalCommaders.FirstOrDefault(x => x.Name == e.Name);

            if (cmdr is null)
            {
                //Something went wrong!!
                return;
            }

            SelectedCommander = cmdr;            
        }

        private void OnCurrentSystemChanged(object? sender, JournalSystem? e)
        {
            OnPropertyChanged(nameof(CurrentSystem));
        }

        private void OnCurrentViewModelChaned()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}
