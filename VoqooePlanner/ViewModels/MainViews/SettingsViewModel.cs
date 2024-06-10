using ODUtils.Commands;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Input;
using VoqooePlanner.Services.Database;
using VoqooePlanner.Stores;
using VoqooePlanner.ViewModels.ModelViews;

namespace VoqooePlanner.ViewModels.MainViews
{
    public sealed class SettingsViewModel : ViewModelBase
    {
        private readonly SettingsStore settingsStore;
        private readonly IVoqooeDatabaseProvider voqooeDatabaseProvider;
        private readonly VoqooeDataStore voqooeDataStore;
        private ObservableCollection<JournalCommaderViewModel> journalCommaderViewModelCollection = [];
        public ObservableCollection<JournalCommaderViewModel> JournalCommaderViews
        {
            get => journalCommaderViewModelCollection;
            set => journalCommaderViewModelCollection = value;
        }
        private bool isloaded;
        public bool IsLoaded { get => isloaded; set { isloaded = value; OnPropertyChanged(nameof(IsLoaded)); } }
        public ICommand OpenPayPal { get; }
        public ICommand SetNewJournalDir { get; }
        public ICommand ToggleCommanderHidden { get; }
        public ICommand ResetLastReadFile { get; }
        public ICommand SaveCommanderChanges { get; }

        private JournalCommaderViewModel? selectedCommander;
        public JournalCommaderViewModel? SelectedCommander { get => selectedCommander; set { selectedCommander = value; OnPropertyChanged(nameof(SelectedCommander)); } }

        public SettingsViewModel(SettingsStore settingsStore, IVoqooeDatabaseProvider voqooeDatabaseProvider, VoqooeDataStore voqooeDataStore)
        {
            this.settingsStore = settingsStore;
            this.voqooeDatabaseProvider = voqooeDatabaseProvider;
            this.voqooeDataStore = voqooeDataStore;
            OpenPayPal = new RelayCommand(OnOpenPayPal);
            SetNewJournalDir = new RelayCommand(OnSetNewDir);

            ToggleCommanderHidden = new RelayCommand(OnToggleCommanderHidden, (_) => SelectedCommander != null);
            ResetLastReadFile = new RelayCommand(OnResetLastFile, (_) => SelectedCommander != null);
            SaveCommanderChanges = new AsyncRelayCommand(OnSaveCommanderChanges, () => SelectedCommander != null);
        }

        public static SettingsViewModel CreateViewModel(SettingsStore settingsStore, IVoqooeDatabaseProvider voqooeDatabaseProvider, VoqooeDataStore voqooeDataStore)
        {
            var vm = new SettingsViewModel(settingsStore, voqooeDatabaseProvider, voqooeDataStore);
            _ = vm.LoadCommanders();
            return vm;
        }
        private void OnToggleCommanderHidden(object? obj)
        {
            if (SelectedCommander != null)
                SelectedCommander.IsHidden = !SelectedCommander.IsHidden;
        }
        private void OnResetLastFile(object? obj)
        {
            if (SelectedCommander != null)
                SelectedCommander.LastFile = string.Empty;
        }

        private async Task OnSaveCommanderChanges()
        {
            if (SelectedCommander == null)
                return;

            var currentCommanders = await voqooeDatabaseProvider.GetAllJournalCommanders(true);

            var cmdr = currentCommanders.FirstOrDefault(x => x.Id == SelectedCommander.Id);

            if (cmdr is null)
            {
                return;
            }

            voqooeDatabaseProvider.AddCommander(new(SelectedCommander.Id, SelectedCommander.Name, SelectedCommander.JournalPath, SelectedCommander.LastFile, SelectedCommander.IsHidden));
            await voqooeDataStore.UpdateCommanders();

            if (SelectedCommander.Id == settingsStore.SelectedCommanderID && cmdr.JournalPath != SelectedCommander.JournalPath)
            {
                voqooeDataStore.ChangeCommander(SelectedCommander.Id);
            }
        }

        private async Task LoadCommanders()
        {
            var commanders = await voqooeDatabaseProvider.GetAllJournalCommanders(true);

            var vms = commanders.Select(x => new JournalCommaderViewModel(x));

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                JournalCommaderViews.Clear();

                foreach (var commander in vms)
                {
                    JournalCommaderViews.Add(commander);
                }

                SelectedCommander = JournalCommaderViews.FirstOrDefault(x => x.Id == settingsStore.SelectedCommanderID);
                OnPropertyChanged(nameof(SelectedCommander));
                OnPropertyChanged(nameof(JournalCommaderViews));

                IsLoaded = true;
            });


        }
        private void OnSetNewDir(object? obj)
        {
            if (SelectedCommander is null)
            {
                return;
            }

            var folderDialog = new FolderBrowserDialog
            {
                InitialDirectory = SelectedCommander.JournalPath
            };

            var result = folderDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                SelectedCommander.JournalPath = folderDialog.SelectedPath;
            }
        }

        private void OnOpenPayPal(object? obj)
        {
            ODUtils.Helpers.OperatingSystem.OpenUrl("https://www.paypal.com/donate/?business=UPEJS3PN7H4XJ&no_recurring=0&item_name=Creator+of+OD+Software.+Thank+you+for+your+donation.&currency_code=GBP");
        }
    }
}
