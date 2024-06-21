using ODUtils.Commands;
using ODUtils.Dialogs;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using VoqooePlanner.EventArgs;
using VoqooePlanner.Services;
using VoqooePlanner.Services.Database;
using VoqooePlanner.Stores;
using VoqooePlanner.ViewModels.ModelViews;

namespace VoqooePlanner.ViewModels.MainViews
{
    public sealed class CartoDataViewModel : ViewModelBase
    {
        private readonly VoqooeDataStore dataStore;
        private readonly EdsmApiService edsmApiService;
        private readonly IVoqooeDatabaseProvider voqooeDatabase;
        private readonly SettingsStore settingsStore;
        private readonly ObservableCollection<StarSystemViewModel> systems = [];

        private bool isloading = false;

        public bool IsLoading { get => isloading; set { isloading = value; OnPropertyChanged(nameof(IsLoading));  } }
        public ObservableCollection<StarSystemViewModel> Systems { get =>  systems; }
        public StarSystemViewModel? SelectedSystem 
        { 
            get => dataStore.SelectedSystem; 
            set {
                dataStore.SelectedSystem = value;
                SelectedBody = dataStore.SelectedSystem?.Bodies.FirstOrDefault();
                OnPropertyChanged(nameof(SelectedSystem));
                OnSelectedSystemChanged?.Invoke(this, SelectedSystem);
            } 
        }
        public SystemBodyViewModel? SelectedBody 
        { 
            get => dataStore.SelectedBody; 
            set {
                dataStore.SelectedBody = value; 
                OnPropertyChanged(nameof(SelectedBody)); 
                OnPropertyChanged(nameof(SelectedBodyIsPlanet)); 
                OnPropertyChanged(nameof(SelectedBodyIsStar)); 
                OnPropertyChanged(nameof(SetIgnoreSystemText)); 
                OnSelectedBodyChanged?.Invoke(this, SelectedBody);
            } 
        }

        public bool SelectedBodyIsPlanet
        {
            get
            {
                if (SelectedBody == null) return false;
                return SelectedBody.IsPlanet;
            }
        }

        public bool SelectedBodyIsStar
        {
            get
            {
                if (SelectedBody == null) return false;
                return SelectedBody.IsStar;
            }
        }

        public string SetIgnoreSystemText
        {
            get
            {
                if (SelectedSystem is null)
                    return "No System Selected";

                return $"Add {SelectedSystem.Name} To Ignore List";
            }
        }

        public string TotalValue => Systems.Sum(x => x.Value).ToString("N0");

        public EventHandler<StarSystemViewModel?>? OnSelectedSystemChanged;
        public EventHandler<SystemBodyViewModel?>? OnSelectedBodyChanged;

        public ICommand OpenEDSM { get; }
        public ICommand OpenSpansh { get; }
        public ICommand AddToIgnoreList { get; }

        public CartoDataViewModel(VoqooeDataStore dataStore,
                                  EdsmApiService edsmApiService,
                                  IVoqooeDatabaseProvider voqooeDatabase,
                                  SettingsStore settingsStore)
        {
            this.dataStore = dataStore;
            this.edsmApiService = edsmApiService;
            this.voqooeDatabase = voqooeDatabase;
            this.settingsStore = settingsStore;
            Task.Factory.StartNew(() => Application.Current.Dispatcher.Invoke(() => BuildSystems(0, 0)));
            dataStore.ReadyStatusChange += OnStoreReady;
            dataStore.OnCartoDataUpdated += OnCartDataUpdated;
            OpenEDSM = new AsyncRelayCommand(OnOpenEDSM, () => SelectedSystem != null);
            OpenSpansh = new RelayCommand(OnOpenSpansh, (_) => SelectedSystem != null);
            AddToIgnoreList = new RelayCommand(OnAddToIgnoreList, (_) => SelectedSystem != null);
        }

        private void OnAddToIgnoreList(object? obj)
        {
            if (SelectedSystem != null)
            {
                voqooeDatabase.AddIgnoreSystem(SelectedSystem.Address, SelectedSystem.Name, settingsStore.SelectedCommanderID);
                dataStore.RemoveSystemFromCartoData(SelectedSystem.Address);
                Systems.Remove(SelectedSystem);
                SelectedSystem = null;
                SelectedBody = null;
                OnPropertyChanged(nameof(Systems));
                OnPropertyChanged(nameof(TotalValue));
            }
        }

        private void OnOpenSpansh(object? obj)
        {
            if (SelectedSystem == null)
                return;

            Process.Start(new ProcessStartInfo($"https://spansh.co.uk/system/{SelectedSystem.Address}") { UseShellExecute = true });        
        }

        private async Task OnOpenEDSM()
        {
            if (SelectedSystem == null)
                return;

            var ret = await edsmApiService.GetSystemUrlAsync(SelectedSystem.Address);

            if (ret != null)
            {
                Process.Start(new ProcessStartInfo(ret) { UseShellExecute = true });
                return;
            }
            _= ODMessageBox.Show(null, "System Not Found", $"EDSM returned no results for address {SelectedSystem.Address}");
        }

        public override void Dispose()
        {
            dataStore.ReadyStatusChange -= OnStoreReady;
            dataStore.OnCartoDataUpdated -= OnCartDataUpdated;
            base.Dispose();
        }

        private void OnCartDataUpdated(object? sender, CartoDataUpdateArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsLoading = true;
                BuildSystems(e.SystemAddress, e.BodyId);
                IsLoading = false;
            });
        }
        private void OnStoreReady(object? sender, bool e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                
                if (!e)
                {
                    IsLoading = true;
                    systems.Clear();
                    SelectedSystem = null;
                    SelectedBody = null;
                    OnPropertyChanged(nameof(Systems));
                    OnPropertyChanged(nameof(TotalValue));
                    return;
                }
                BuildSystems();                
            });
        }

        private void BuildSystems(long systemAddress = 0, long bodyid = 0)
        {
            Systems.Clear();

            var systemsToAdd = dataStore.CartoSystems.Where(x => x.SystemBodies.Count > 0);
            foreach (var system in systemsToAdd)
            {
                Systems.Add(new(system));
            }

            if(systemAddress != 0) 
                SetSelectedItems(systemAddress, bodyid);
            OnPropertyChanged(nameof(Systems));
            OnPropertyChanged(nameof(TotalValue));
            IsLoading = false;
        }

        private void SetSelectedItems(long systemAddress = 0, long bodyid = 0)
        {
            if (systemAddress == 0)
            {
                SelectedSystem = systems.FirstOrDefault();
                SelectedBody = SelectedSystem?.Bodies.FirstOrDefault();
                return;
            }

            var known = systems.FirstOrDefault(x => x.Address == systemAddress);

            if (known != null)
            {
                SelectedSystem = known;
                SelectedBody = known?.Bodies.FirstOrDefault(x => x.Id == bodyid);
                return;
            }

            SelectedSystem = null;
            SelectedBody = null;
        }
    }
}
