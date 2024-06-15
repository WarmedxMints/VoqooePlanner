using System.Collections.ObjectModel;
using System.Windows;
using VoqooePlanner.EventArgs;
using VoqooePlanner.Stores;
using VoqooePlanner.ViewModels.ModelViews;

namespace VoqooePlanner.ViewModels.MainViews
{
    public sealed class CartoDataViewModel : ViewModelBase
    {
        private readonly VoqooeDataStore dataStore;
        private readonly ObservableCollection<StarSystemViewModel> systems = [];
        private StarSystemViewModel? selectedSystem;
        private SystemBodyViewModel? selectedBody;
        private bool isloading = false;

        public bool IsLoading { get => isloading; set { isloading = value; OnPropertyChanged(nameof(IsLoading));  } }
        public ObservableCollection<StarSystemViewModel> Systems { get =>  systems; }
        public StarSystemViewModel? SelectedSystem 
        { 
            get => selectedSystem; 
            set { 
                selectedSystem = value;
                SelectedBody = selectedSystem?.Bodies.FirstOrDefault();
                OnPropertyChanged(nameof(SelectedSystem));
                OnSelectedSystemChanged?.Invoke(this, SelectedSystem);
            } 
        }
        public SystemBodyViewModel? SelectedBody 
        { 
            get => selectedBody; 
            set { 
                selectedBody = value; 
                OnPropertyChanged(nameof(SelectedBody)); 
                OnSelectedBodyChanged?.Invoke(this, SelectedBody);
            } 
        }
        public string TotalValue => Systems.Sum(x => x.Value).ToString("N0");

        public EventHandler<StarSystemViewModel?>? OnSelectedSystemChanged;
        public EventHandler<SystemBodyViewModel?>? OnSelectedBodyChanged;

        public CartoDataViewModel(VoqooeDataStore dataStore)
        {
            this.dataStore = dataStore;
            Task.Factory.StartNew(() => Application.Current.Dispatcher.Invoke(() => BuildSystems(0, 0)));
            dataStore.ReadyStatusChange += OnStoreReady;
            dataStore.OnCartoDataUpdated += OnCartDataUpdated;
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
            systems.Clear();

            foreach(var system in dataStore.CartoSystems)
            {
                systems.Add(new(system));
            }

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
                SelectedBody = selectedSystem?.Bodies.FirstOrDefault();
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
