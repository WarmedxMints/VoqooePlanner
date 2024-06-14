using ODUtils.Commands;
using ODUtils.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;
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
        private readonly JournalWatcherStore watcherStore;
        private ObservableCollection<JournalCommaderViewModel> journalCommaderViewModelCollection = [];
        public ObservableCollection<JournalCommaderViewModel> JournalCommaderViews
        {
            get => journalCommaderViewModelCollection;
            set => journalCommaderViewModelCollection = value;
        }

        private bool isloaded;
        public bool IsLoaded { get => isloaded; set { isloaded = value; OnPropertyChanged(nameof(IsLoaded)); } }

        private Visibility scanningWindowVisibility = Visibility.Collapsed;
        public Visibility ScanningWindowVisibility { get => scanningWindowVisibility; set { scanningWindowVisibility = value; OnPropertyChanged(nameof(ScanningWindowVisibility)); } }

        private string directoryScanText = string.Empty;
        public string DirectoryScanText
        {
            get => directoryScanText;
            set
            {
                directoryScanText = value;
                OnPropertyChanged(nameof(DirectoryScanText));
            }
        }

        private string fileReadingText = string.Empty;
        public string FileReadingText
        {
            get => fileReadingText;
            set
            {
                fileReadingText = value;
                OnPropertyChanged(nameof(FileReadingText));
            }
        }


        public ICommand OpenPayPal { get; }
        public ICommand SetNewJournalDir { get; }
        public ICommand ToggleCommanderHidden { get; }
        public ICommand ResetLastReadFile { get; }
        public ICommand SaveCommanderChanges { get; }
        public ICommand ScanNewDirectory { get; }
        public ICommand ResetDataBaseCommand { get; }

        private JournalCommaderViewModel? selectedCommander;
        public JournalCommaderViewModel? SelectedCommander { get => selectedCommander; set { selectedCommander = value; OnPropertyChanged(nameof(SelectedCommander)); } }

        public SettingsViewModel(SettingsStore settingsStore,
                                 IVoqooeDatabaseProvider voqooeDatabaseProvider,
                                 VoqooeDataStore voqooeDataStore,
                                 JournalWatcherStore watcherStore)
        {
            this.settingsStore = settingsStore;
            this.voqooeDatabaseProvider = voqooeDatabaseProvider;
            this.voqooeDataStore = voqooeDataStore;
            this.watcherStore = watcherStore;

            this.voqooeDataStore.ReadyStatusChange += OnReadyStateChange;
            this.watcherStore.OnReadingNewFile += OnReadingNewFile;

            OpenPayPal = new RelayCommand(OnOpenPayPal);
            SetNewJournalDir = new RelayCommand(OnSetNewDir, (_) => IsLoaded);
            ToggleCommanderHidden = new RelayCommand(OnToggleCommanderHidden, (_) => IsLoaded && SelectedCommander != null);
            ResetLastReadFile = new RelayCommand(OnResetLastFile, (_) => IsLoaded && SelectedCommander != null);
            SaveCommanderChanges = new AsyncRelayCommand(OnSaveCommanderChanges, () => IsLoaded && SelectedCommander != null);
            ScanNewDirectory = new AsyncRelayCommand(OnScanNewDirectory, () => IsLoaded);
            ResetDataBaseCommand = new AsyncRelayCommand(OnResetDataBase, () => IsLoaded);
        }

        public override void Dispose()
        {
            voqooeDataStore.ReadyStatusChange -= OnReadyStateChange;
            watcherStore.OnReadingNewFile -= OnReadingNewFile;
        }

        private async Task OnResetDataBase()
        {
            var msgBox = ODMessageBox.Show(System.Windows.Application.Current.MainWindow,
                                           "Reset Database",
                                           "This will reset all Commander data and scan the default directory\n\nAre you sure?",
                                           MessageBoxButton.YesNo);

            if (msgBox != MessageBoxResult.Yes)
            {
                return;
            }

            ScanningWindowVisibility = Visibility.Visible;
            DirectoryScanText = $"Resetting Database";
            JournalCommaderViews.Clear();
            SelectedCommander = null;
            OnPropertyChanged(nameof(SelectedCommander));
            OnPropertyChanged(nameof(JournalCommaderViews));
            await Task.Factory.StartNew(voqooeDatabaseProvider.ResetDataBaseAsync);
            settingsStore.SelectedCommanderID = 0;
            DirectoryScanText = $"Scanning Default Directory";
            voqooeDataStore.PerformFirstRun();
        }

        private void OnReadingNewFile(object? sender, string e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() => FileReadingText = $"Reading {e}");
        }

        private void OnReadyStateChange(object? sender, bool e)
        {
            isloaded = e;
            if (e)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    FileReadingText = string.Empty;
                    DirectoryScanText = string.Empty;
                    ScanningWindowVisibility = Visibility.Collapsed;
                    Task.Run(LoadCommanders);
                });
            }
        }

        private async Task OnScanNewDirectory()
        {
            var folderDialog = new FolderBrowserDialog();
            var result = folderDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                DirectoryScanText = $"Scanning {folderDialog.SelectedPath}";
                ScanningWindowVisibility = Visibility.Visible;
                await voqooeDataStore.ScanNewDirctory(folderDialog.SelectedPath);
            }
        }

        public static SettingsViewModel CreateViewModel(SettingsStore settingsStore,
                                                        IVoqooeDatabaseProvider voqooeDatabaseProvider,
                                                        VoqooeDataStore voqooeDataStore,
                                                        JournalWatcherStore watcherStore)
        {
            var vm = new SettingsViewModel(settingsStore, voqooeDatabaseProvider, voqooeDataStore, watcherStore);
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


            foreach (var cmdr in JournalCommaderViews)
            {
                voqooeDatabaseProvider.AddCommander(new(cmdr.Id, cmdr.Name, cmdr.JournalPath, cmdr.LastFile, cmdr.IsHidden));
            }
            await voqooeDataStore.UpdateCommanders();

            var currentCommanders = await voqooeDatabaseProvider.GetAllJournalCommanders(true);

            var selectedCmdr = currentCommanders.FirstOrDefault(x => x.Id == SelectedCommander.Id);

            if (selectedCmdr is null)
            {
                return;
            }

            if (SelectedCommander.Id == settingsStore.SelectedCommanderID && selectedCmdr.JournalPath != SelectedCommander.JournalPath)
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
                SelectedCommander.LastFile = string.Empty;
            }
        }

        private void OnOpenPayPal(object? obj)
        {
            ODUtils.Helpers.OperatingSystem.OpenUrl("https://www.paypal.com/donate/?business=UPEJS3PN7H4XJ&no_recurring=0&item_name=Creator+of+OD+Software.+Thank+you+for+your+donation.&currency_code=GBP");
        }
    }
}
