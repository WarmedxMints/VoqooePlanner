
using EliteJournalReader;
using ODUtils.Commands;
using ODUtils.Models;
using ODUtils.Wpf3D;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using VoqooePlanner.Commands;
using VoqooePlanner.Models;
using VoqooePlanner.Stores;
using ODUtils.Dialogs;
using ODUtils.PathFinding;
using VoqooePlanner.Services.Database;

namespace VoqooePlanner.ViewModels
{
    public sealed class VoqooeListViewModel : ViewModelBase
    {
		private bool isLoading = true;
        public bool IsLoading { get => isLoading; set { isLoading = value; OnPropertyChanged(nameof(IsLoading)); } }

        private readonly VoqooeDataStore voqooeDataStore;
        private readonly IVoqooeDatabaseProvider voqooeDatabaseProvider;
        private readonly SettingsStore settings;
        private readonly JournalWatcherStore journalWatcher;
        private readonly ObservableCollection<VoqooeSystemViewModel> voqooeSystems;
        private readonly List<RouteStopViewModel> route;

        private RouteStopViewModel? selectedItem;

        private string readingFileText = string.Empty;
        public string ReadingFileText { get => readingFileText; set { readingFileText = value; OnPropertyChanged(nameof(ReadingFileText)); } }
        
        private string distance = "0 ly";
        public string Distance { get => distance; set { distance = value; OnPropertyChanged(nameof(Distance)); } }

        public List<int> SelectedStarClasses
        {
            get => settings.StarClassFilter;
            set
            {
                settings.StarClassFilter = value;
                OnPropertyChanged(nameof(SelectedStarClasses));
            }
        }

        public RouteStopViewModel? SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));

                if (selectedItem != null)
                {
                    OnSeletedItemChanged?.Invoke(this, selectedItem);
                }
            }
        }

        public NearBySystemsOptions NearSystemOptions
        {
            get => settings.NearBySystemsOptions;
            set
            {
                settings.NearBySystemsOptions = value;
                voqooeDataStore.UpdateNearByOptions();
                OnPropertyChanged(nameof(NearSystemOptions));
            }
        }

        public bool AutoSelectNextSystem
        {
            get => settings.AutoSelectNextSystemInRoute;
            set
            {
                settings.AutoSelectNextSystemInRoute = value;
                OnPropertyChanged(nameof(NearSystemOptions));
            }
        }

        public bool AutoCopyNextSystem
        {
            get => settings.AutoCopyNextSystemToClipboard;
            set
            {
                settings.AutoCopyNextSystemToClipboard = value;
                OnPropertyChanged(nameof(AutoCopyNextSystem));
            }
        }

        public IEnumerable<RouteStopViewModel> Route => route;
        public IEnumerable<VoqooeSystemViewModel> VoqooeSystems => voqooeSystems;

        private JournalSystemViewModel? currentSystem;
        public JournalSystemViewModel? CurrentSystem
        {
            get => currentSystem;
            set
            {
                currentSystem = value;
                OnPropertyChanged(nameof(CurrentSystem));
            }
        }

        public ICommand LoadSphereDataCommand { get; }       
        public ICommand ZoomMapOut { get; }
        public ICommand ZoomMapIn { get; }
        public ICommand MapLeft { get; }
        public ICommand MapRight { get; }
        public ICommand MapUp { get; }
        public ICommand MapDown { get; }
        public ICommand GenerateRoute { get; }
        public ICommand ChangeSelectedItem { get; }
        public ICommand CopyStringToClipboard { get; }
        public ICommand UpdateNearby { get; }

        public Action? RefreshGrid;
        public EventHandler<RouteStopViewModel>? OnSeletedItemChanged;
        public EventHandler<string>? OnStringCopiedToClipboard;
        public VoqooeListViewModel(VoqooeDataStore voqooeDataStore, IVoqooeDatabaseProvider voqooeDatabaseProvider, SettingsStore settings, JournalWatcherStore journalWatcher)
		{
            this.voqooeDataStore = voqooeDataStore;
            this.voqooeDatabaseProvider = voqooeDatabaseProvider;
            this.settings = settings;
            this.journalWatcher = journalWatcher;
            voqooeSystems = [];
            route = [];
           
            LoadSphereDataCommand = new LoadVoqooeDataCommand(this, voqooeDataStore);

            voqooeDataStore.OnSystemsUpdated += OnSystemsUpdated;
            voqooeDataStore.OnCurrentSystemChanged += OnCurrentSystemChanged;
            voqooeDataStore.ReadyStatusChange += OnReadyStateChange;
            journalWatcher.OnReadingNewFile += OnReadingNewFile;
            //commands
            ZoomMapOut = new RelayCommand(OnMapZoomOut, (_) => ModelGroup.Children.Count > 0);
            ZoomMapIn = new RelayCommand(OnZoomMapIn, (_) => ModelGroup.Children.Count > 0);
            MapLeft = new RelayCommand(OnMapLeft, (_) => ModelGroup.Children.Count > 0);
            MapRight = new RelayCommand(OnMapRight, (_) => ModelGroup.Children.Count > 0);
            MapUp = new RelayCommand(OnMapUp, (_) => ModelGroup.Children.Count > 0);
            MapDown = new RelayCommand(OnMapDown, (_) => ModelGroup.Children.Count > 0);
            GenerateRoute = new AsyncRelayCommand(OnGenerateRoute, () => voqooeSystems.Any() && !calculatingRoute);
            ChangeSelectedItem = new RelayCommand<bool>(OnChangeSelectedItem, (_) => Route.Any());
            CopyStringToClipboard = new RelayCommand<string?>(OnCopyStringToClipboard);
            UpdateNearby = new RelayCommand(OnUpdateNearby);
        }

        private void OnReadingNewFile(object? sender, string e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ReadingFileText = $"Reading File {e}...";
            });
        }

        private void OnUpdateNearby(object? obj)
        {
            voqooeDataStore.UpdateNearByOptions();
        }

        private void OnCopyStringToClipboard(string? obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (obj == null)
                    return;

                ODUtils.Helpers.ClipboardHelper.SetStringToClipboard(obj);
                OnStringCopiedToClipboard?.Invoke(this, obj);
            });
        }

        private void OnChangeSelectedItem(bool up)
        {
            ChangeRow(up ? -1 : 1);
        }

        private void OnReadyStateChange(object? sender, bool e)
        {
            OnPropertyChanged(nameof(NearSystemOptions));
            OnPropertyChanged(nameof(AutoCopyNextSystem));
            OnPropertyChanged(nameof(AutoSelectNextSystem));
            OnPropertyChanged(nameof(SelectedStarClasses));
            IsLoading = !e;
        }      

        private void OnCurrentSystemChanged(object? sender, JournalSystem? e)
        {
            var hub = voqooeDataStore.HubSystem ?? new VoqooeSystem(0, string.Empty, 0, 0, 0, false, false, 0, 0);
            var distance = SystemPosition.Distance(e?.Pos ?? new(), new() { X = hub.X, Y = hub.Y, Z = hub.Z });
            CurrentSystem = new(e ?? new(0, new(), "Unknown"), distance);

            if(SelectedItem != null && e != null && AutoSelectNextSystem && e.Name == SelectedItem.Name)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ChangeRow(1);
                });
            }
        }

        private void OnSystemsUpdated(object? sender, EventArgs e)
        {
            UpdateSystemList();
        }

        public static VoqooeListViewModel CreateModel(VoqooeDataStore voqooeDataStore, IVoqooeDatabaseProvider voqooeDatabaseProvider, SettingsStore settings, JournalWatcherStore journalWatcher)
        {
            var ret = new VoqooeListViewModel(voqooeDataStore, voqooeDatabaseProvider, settings, journalWatcher);
            Task.Run(() => ret.LoadSphereDataCommand.Execute(null));
            if(voqooeDataStore.Ready)
            {
                ret.OnCurrentSystemChanged(null, voqooeDataStore.CurrentSystem);
            }
            return ret;
        }

        internal void UpdateSystemList()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                voqooeSystems.Clear();

                foreach (var system in voqooeDataStore.VoqooeSystems)
                {
                    var viewModel = new VoqooeSystemViewModel(system);
                    voqooeSystems.Add(viewModel);
                }
                OnPropertyChanged(nameof(VoqooeSystems));
            });
        }

        private async Task OnGenerateRoute()
        {
            if (VoqooeSystems.Any() == false || CurrentSystem == null)
            {
                return;
            }

            var startSystem = voqooeDatabaseProvider.GetVoqooeSystemByName(CurrentSystem.Name);

            if (startSystem is null)
            {
                _ = ODMessageBox.Show(null, "Error", $"Start System {CurrentSystem.Name} not found in the database", System.Windows.MessageBoxButton.OK);
                return;
            }

            
            var stops = new List<RouteStop>();


            calculatingRoute = true;
            var firtStop = new RouteStop(new Position(startSystem.X, startSystem.Y, startSystem.Z), new VoqooeSystemViewModel(startSystem));

            foreach (var sys in VoqooeSystems)
            {
                stops.Add(new(sys.Pos, sys));
            }

            var routeCalc = new TravellingSalesmAlgorithm(firtStop, stops);
            var routeStops = await routeCalc.GetRouteAsync();

            if (routeStops == null)
            {
                return;
            }
            stops.Clear();
            stops = routeStops.ToList();

            var count = stops.Count;
            double totalDistance = 0;
            route.Clear();

            route.Add(new RouteStopViewModel((VoqooeSystemViewModel)stops[0].System));

            for (var i = 1; i < count; i++)
            {
                var modelToAdd = new RouteStopViewModel((VoqooeSystemViewModel)stops[i].System, (VoqooeSystemViewModel)stops[i - 1].System);
                route.Add(modelToAdd);
                totalDistance += modelToAdd.Distance;
            }

            var endSystem = new RouteStopViewModel((VoqooeSystemViewModel)stops[0].System, (VoqooeSystemViewModel)stops[count - 1].System);
            route.Add(endSystem);
            totalDistance += endSystem.Distance;

            Distance = $"{totalDistance:N2} ly";
            //SystemCount = $"{route.Count - 1:N0}";
            SelectedItem = route[0];
            OnPropertyChanged(nameof(Route));

            CreateMap();
            ChangeRow(1);
            calculatingRoute = false;
        }

        private void ChangeRow(int value)
        {
            if (selectedItem == null)
            {
                return;
            }

            var newIndex = route.IndexOf(selectedItem) + value;

            var count = route.Count - 1;

            if (newIndex < 1)
            {
                newIndex = count - 1;
            }
            if (newIndex > count)
            {
                newIndex = 1;
            }
            var item = route[newIndex];
            SelectedItem = item;
            ColourMap(newIndex);
            if (AutoCopyNextSystem)
            {
                OnCopyStringToClipboard(SelectedItem.Name);
            }
        }

        #region 3D map
        private Model3DGroup modelGroup = new();
        public Model3DGroup ModelGroup { get => modelGroup; set { modelGroup = value; OnPropertyChanged(); } }
        private Dictionary<string, GeometryModel3D> SystemSpheres = [];
        private Dictionary<string, GeometryModel3D> SystemLines = [];

        readonly DiffuseMaterial green = new(new SolidColorBrush(Colors.Green));
        readonly DiffuseMaterial grey = new(new SolidColorBrush(Colors.LightSlateGray));
        readonly DiffuseMaterial red = new(new SolidColorBrush(Colors.Red));
        readonly DiffuseMaterial blue = new(new SolidColorBrush(Colors.SkyBlue));
        readonly DiffuseMaterial axisblue = new(new SolidColorBrush(Colors.Blue));
        // The camera.
        private readonly PerspectiveCamera TheCamera = new();
        public PerspectiveCamera PerspectiveCamera { get => TheCamera; }
        // The camera's current location.
        private double CameraPhi = Math.PI / 3.0;
        private double CameraTheta = Math.PI / 3.0;
        private double CameraR = 3.0;
        private bool calculatingRoute;

        // The change in CameraPhi when you press the up and down arrows.
        private const double CameraDPhi = 0.1;

        // The change in CameraTheta when you press the left and right arrows.
        private const double CameraDTheta = 0.1;

        // The change in CameraR when you press + or -.
        private const double CameraDR = 5;

        private void CreateMap()
        {
            if (route.Count == 0)
            {
                return;
            }

            var routeCount = route.Count;
            SystemSpheres = [];
            SystemLines = [];

            var centroid = new Position(0, 0, 0);

            foreach (var system in route)
            {
                centroid += system.Pos;
            }

            centroid /= routeCount;

            Model3DGroup gr = new();
            gr.Children.Add(new AmbientLight());

            //Axis arrows
            //var line3 = MeshGeneration.CreatBoxLine(new Position(0, -0.5, 0), new Position(0, 5, 0), green, 1, true);
            //gr.Children.Add(line3);
            //line3 = MeshGeneration.CreatBoxLine(new Position(0.5, 0, 0), new Position(5, 0, 0), red, 1, true);
            //gr.Children.Add(line3);
            //line3 = MeshGeneration.CreatBoxLine(new Position(0, 0, -0.5), new Position(0, 0, -5), axisblue, 1, true);
            //gr.Children.Add(line3);

            for (var i = 0; i < routeCount - 1; i++)
            {
                var sys = route[i];
                var start = sys.Pos - centroid;
                //ED uses a Z+ system where as wpf seems to use a Z- so we need to flip it
                start = start.FlipZ;
                var sphere = MeshGeneration.CreateSphere(start, grey, 2, 10, 10);
                SystemSpheres.Add(sys.Name, sphere);
                gr.Children.Add(sphere);
                Position end = new(0, 0, 0);
                if (i == routeCount - 2)
                {
                    end = route[0].Pos - centroid;
                    end = end.FlipZ;
                    var line = MeshGeneration.CreatBoxLine(start, end, grey, 1);
                    SystemLines.Add(sys.Name, line);
                    gr.Children.Add(line);
                    continue;
                }
                end = route[i + 1].Pos - centroid;
                end = end.FlipZ;
                var line1 = MeshGeneration.CreatBoxLine(start, end, grey, 1);
                gr.Children.Add(line1);
                SystemLines.Add(sys.Name, line1);
            }

            var maxDistance = route.Max(x => x.Pos.DistanceFrom(centroid));
            CameraR = maxDistance * 3;
            CameraTheta = Math3d.ConvertToRadians(90);// 3.14159;
            CameraPhi = 0;
            PositionCamera();
            ModelGroup = gr;
        }

        private void ColourMap(int index)
        {
            GetSystemNames(index, out string? fromSystem, out string? toSystem);

            foreach (var sphere in SystemSpheres)
            {
                if (sphere.Key.Equals(fromSystem))
                {
                    sphere.Value.Material = green;
                    continue;
                }
                if (sphere.Key.Equals(toSystem))
                {
                    sphere.Value.Material = red;
                    continue;
                }
                sphere.Value.Material = grey;
            }

            foreach (var system in SystemLines)
            {
                if (system.Key.Equals(fromSystem))
                {
                    system.Value.Material = blue;
                    continue;
                }
                system.Value.Material = grey;
            }
        }

        private void GetSystemNames(int index, out string fromSystem, out string toSystem)
        {
            if (index > 0 && index < route.Count)
            {
                fromSystem = route[index - 1].Name;
                toSystem = route[index].Name;
                return;
            }
            if (index == 0)
            {
                fromSystem = route[index].Name;
                toSystem = route[index + 1].Name;
                return;
            }
            fromSystem = route[index - 1].Name;
            toSystem = route[index].Name;
        }

        private void PositionCamera()
        {
            if (TheCamera == null) return;
            // Calculate the camera's position in Cartesian coordinates.
            double y = CameraR * Math.Sin(CameraPhi);
            double hyp = CameraR * Math.Cos(CameraPhi);
            double x = hyp * Math.Cos(CameraTheta);
            double z = hyp * Math.Sin(CameraTheta);
            TheCamera.Position = new Point3D(x, y, z);
            // Look toward the origin.
            TheCamera.LookDirection = new Vector3D(-x, -y, -z);
            // Set the Up direction.
            TheCamera.UpDirection = new Vector3D(0, 1, 0);
        }

        private void OnMapDown(object? obj)
        {
            CameraPhi -= CameraDPhi;
            if (CameraPhi < -Math.PI / 2.0) CameraPhi = -Math.PI / 2.0;
            PositionCamera();
        }

        private void OnMapUp(object? obj)
        {
            CameraPhi += CameraDPhi;
            if (CameraPhi > Math.PI / 2.0) CameraPhi = Math.PI / 2.0;
            PositionCamera();
        }

        private void OnMapRight(object? obj)
        {
            CameraTheta -= CameraDTheta;
            PositionCamera();
        }

        private void OnMapLeft(object? obj)
        {
            CameraTheta += CameraDTheta;
            PositionCamera();
        }

        private void OnZoomMapIn(object? obj)
        {
            CameraR -= CameraDR;
            if (CameraR < CameraDR) CameraR = CameraDR;
            PositionCamera();
        }

        private void OnMapZoomOut(object? obj)
        {
            CameraR += CameraDR;
            PositionCamera();
        }
        #endregion
    }
}