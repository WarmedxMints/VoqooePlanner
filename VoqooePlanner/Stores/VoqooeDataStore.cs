using EliteJournalReader;
using EliteJournalReader.Events;
using ODUtils.Dialogs;
using System.Collections.Concurrent;
using VoqooePlanner.EventArgs;
using VoqooePlanner.Models;
using VoqooePlanner.Services.Database;
using VoqooePlanner.ViewModels.ModelViews;

namespace VoqooePlanner.Stores
{
    public sealed class VoqooeDataStore
    {
        private readonly IVoqooeDatabaseProvider voqooeDatabaseProvider;
        private readonly JournalWatcherStore journalWatcherStore;
        private readonly SettingsStore settingsStore;
        private readonly LoggerStore loggerStore;
        private Lazy<Task> _initializeLazy;

        private readonly List<VoqooeSystem> _voqooeSystems;
        private readonly List<JournalCommander> _journalCommanders;
        private readonly ConcurrentDictionary<int, List<VoqooeSystem>> _systemVisitsToAdd = [];
        private readonly ConcurrentDictionary<long, StarSystem> _voqooeCartoData = [];
        private readonly List<RouteStopViewModel> route = [];

        private RouteStopViewModel? selectedItem;

        public List<RouteStopViewModel> Route => route;
        public RouteStopViewModel? SelectedItem { get => selectedItem; set { selectedItem = value; } }
        public IEnumerable<JournalCommander> JournalCommaders => _journalCommanders;

        private JournalCommander _currentCommander = new(0, string.Empty, string.Empty, string.Empty, false);

        private JournalSystem? currentSystem;
        public JournalSystem? CurrentSystem
        {
            get => currentSystem;
            private set
            {
                currentSystem = value;
                if (Ready == false)
                    return;

                OnCurrentSystemChanged?.Invoke(this, currentSystem);
            }
        }

        public string CurrentBodyName { get; private set; } = string.Empty;
        public VoqooeSystem? HubSystem { get; private set; }
        public IEnumerable<VoqooeSystem> VoqooeSystems => _voqooeSystems;
        public IEnumerable<JournalCommander> JournalCommanders => _journalCommanders;
        public IEnumerable<StarSystem> CartoSystems => _voqooeCartoData.Values;

        private readonly Dictionary<string, OrganicScanDetails> bioData;
        public Dictionary<string, OrganicScanDetails> BioData => bioData;

        private readonly List<OrganicDetails> _organicDataToSell;
        public IEnumerable<OrganicDetails> OrganicDataToSell => _organicDataToSell;

        public bool Loaded { get; private set; }
        public bool Ready { get; private set; }
        private bool FirstRun { get; set; }
        #region Events
        public EventHandler? OnCommandersUpdated;
        public EventHandler<JournalCommander>? OnCurrentCommanderChanged;
        public EventHandler<bool>? ReadyStatusChange;
        public EventHandler? OnSystemsUpdated;
        public EventHandler<JournalSystem?>? OnCurrentSystemChanged;
        public EventHandler<IEnumerable<OrganicScanDetails>>? OnOrganicDataChanged;
        public EventHandler? OnOrganicToSellDataChanged;
        public EventHandler<CartoDataUpdateArgs>? OnCartoDataUpdated;
        #endregion

        public VoqooeDataStore(IVoqooeDatabaseProvider voqooeDatabaseProvider, JournalWatcherStore journalWatcherStore, SettingsStore settingsStore, LoggerStore loggerStore)
        {
            this.voqooeDatabaseProvider = voqooeDatabaseProvider;
            this.journalWatcherStore = journalWatcherStore;
            this.settingsStore = settingsStore;
            this.loggerStore = loggerStore;
            _initializeLazy = new Lazy<Task>(Initialise);
            _voqooeSystems = [];
            _journalCommanders = [];
            bioData = [];
            _organicDataToSell = [];

            journalWatcherStore.OnJournalEventRecieved += OnJournalEntry;
            journalWatcherStore.LiveStatusChange += OnLiveStatusChange;
        }

        private void OnLiveStatusChange(object? sender, bool e)
        {
            Ready = e && Loaded;
            if (e && FirstRun)
            {
                Task.Factory.StartNew(FinishFirstRun);
                return;
            }
            if(e == false)
            {
                ReadyStatusChange?.Invoke(this, Ready);
                return;
            }
            Task.Run(async () =>
            {
                if (!_systemVisitsToAdd.IsEmpty)
                {
                    foreach (var system in _systemVisitsToAdd)
                    {
                        voqooeDatabaseProvider.AddCommanderVisits(system.Value, system.Key);
                    }
                    _systemVisitsToAdd.Clear();
                }

                await UpdateCommanders();
                await UpdateSystems();
                OnCurrentSystemChanged?.Invoke(this, currentSystem);
                OnCurrentCommanderChanged?.Invoke(this, _currentCommander);

                ReadyStatusChange?.Invoke(this, Ready);
            });
        }

        private void OnJournalEntry(object? sender, JournalEntry e)
        {
            if (e.EventType == JournalTypeEnum.Commander)
            {
                if (!FirstRun && e.EventData is CommanderEvent.CommanderEventArgs commanderEvent
                    && _journalCommanders.FirstOrDefault(x => x.Name == commanderEvent.Name) == null)
                {
                    _ = UpdateCommanders();
                    return;
                }
            }
            ProcessJournalEntry(e);
        }

        private void ProcessJournalEntry(JournalEntry e)
        {
            switch (e.EventType)
            {
                case JournalTypeEnum.Location:
                    if (e.EventData is LocationEvent.LocationEventArgs location)
                    {

                        if (e.CommanderID == _currentCommander.Id)
                        {
                            CurrentSystem = new(location.SystemAddress, location.StarPos.Copy(), location.StarSystem);
                           
                            if (location.StarSystem.StartsWith("Voqooe") && _voqooeCartoData.TryAdd(location.SystemAddress, new(location.SystemAddress, location.StarSystem, location.StarPos.Copy(), StarType.Unknown)))
                            {
                                if(Ready)
                                    OnCartoDataUpdated?.Invoke(this, new(location.SystemAddress, location.BodyID));
                            }
                            if (Ready)
                            {                               
                                _ = UpdateSystems().ConfigureAwait(false);
                            }
                        }
                    }
                    break;
                case JournalTypeEnum.FSDJump:
                    if (e.EventData is FSDJumpEvent.FSDJumpEventArgs fsdJump)
                    {
                        if (e.CommanderID == _currentCommander.Id)
                        {
                            CurrentSystem = new(fsdJump.SystemAddress, fsdJump.StarPos.Copy(), fsdJump.StarSystem);
                            if (Ready)
                            {
                                Task.Run(UpdateSystems);
                            }
                        }
                        if (fsdJump.StarSystem.StartsWith("Voqooe", StringComparison.OrdinalIgnoreCase) == false)
                        {
                            break;
                        }

                        if (_voqooeCartoData.TryAdd(fsdJump.SystemAddress, new(fsdJump.SystemAddress, fsdJump.StarSystem, fsdJump.StarPos.Copy(), StarType.Unknown)))
                        {
                            if (Ready)
                                OnCartoDataUpdated?.Invoke(this, new(fsdJump.SystemAddress, fsdJump.BodyID));
                        }
                        var voqooeSystem = new VoqooeSystem(fsdJump.SystemAddress, fsdJump.StarSystem, fsdJump.StarPos.X, fsdJump.StarPos.Y, fsdJump.StarPos.Z, true, false, 0, 0);

                        if (Ready)
                        {
                            voqooeDatabaseProvider.AddCommanderVisit(voqooeSystem, e.CommanderID);
                            break;
                        }
                        if (_systemVisitsToAdd.ContainsKey(e.CommanderID) == false)
                        {
                            _systemVisitsToAdd[e.CommanderID] = [voqooeSystem];
                            break;
                        }
                        _systemVisitsToAdd[e.CommanderID].Add(voqooeSystem);
                    }
                    break;
                case JournalTypeEnum.NavRoute:
                    //This json can only be read when live as it is replaced each time a route is created
                    if (Ready && e.CommanderID == _currentCommander.Id)
                    {
                        var evt = journalWatcherStore.ReadNavRouteJson(_currentCommander.Id);

                        if (evt is null)
                            break;

                        var systemsToUpdate = new List<VoqooeSystem>();
                        foreach (var system in evt.Route)
                        {
                            if (!system.StarSystem.StartsWith("Voqooe"))
                            {
                                break;
                            }
                            if (_voqooeCartoData.TryAdd(system.SystemAddress, new(system.SystemAddress, system.StarSystem, system.StarPos.Copy(), system.StarClass)))
                            {
                                //TODO AddUpdate Event
                            }

                            var voqooeSystem = new VoqooeSystem(system.SystemAddress, system.StarSystem, system.StarPos.X, system.StarPos.Y, system.StarPos.Z, false, false, (int)system.StarClass, 0);
                            systemsToUpdate.Add(voqooeSystem);
                        }

                        if (systemsToUpdate.Count > 0)
                        {
                            voqooeDatabaseProvider.UpdateVoqooeSystems(systemsToUpdate);
                        }
                    }
                    break;
                case JournalTypeEnum.FSSDiscoveryScan:
                    if (e.CommanderID == _currentCommander.Id
                        && e.EventData is FSSDiscoveryScanEvent.FSSDiscoveryScanEventArgs fssScan)
                    {
                        if (!fssScan.SystemName.StartsWith("Voqooe"))
                        {
                            break;
                        }

                        if (_voqooeCartoData.TryGetValue(fssScan.SystemAddress, out var value))
                        {
                            if (value is null)
                                break;

                            value.BodyCount = fssScan.BodyCount;

                            //TODO Fire system update event
                        }
                    }
                    break;
                case JournalTypeEnum.Scan:
                    if (e.CommanderID == _currentCommander.Id
                        && e.EventData is ScanEvent.ScanEventArgs scanEvt)
                    {
                        if (_voqooeCartoData.TryGetValue(scanEvt.SystemAddress, out var value))
                        {
                            if (value is null)
                                break;

                            value.AddBody(scanEvt);

                            if (Ready)
                                OnCartoDataUpdated?.Invoke(this, new(scanEvt.SystemAddress, scanEvt.BodyID));
                        }
                    }
                    break;
                case JournalTypeEnum.SAAScanComplete:
                    if (e.CommanderID == _currentCommander.Id 
                        && e.EventData is SAAScanCompleteEvent.SAAScanCompleteEventArgs saaScan)
                    {
                        if (_voqooeCartoData.TryGetValue(saaScan.SystemAddress, out var value))
                        {
                            if (value is null)
                                break;

                            value.UpdateBodyFromDSS(saaScan);

                            //TODO Fire update event
                        }
                    }
                    break;
                case JournalTypeEnum.SellExplorationData:
                    if(e.CommanderID == _currentCommander.Id
                        && e.EventData is SellExplorationDataEvent.SellExplorationDataEventArgs sellCarto)
                    {
                        foreach (string system in sellCarto.Systems)
                        {
                            var known = _voqooeCartoData.FirstOrDefault(x => x.Value.Name.Equals(system, StringComparison.OrdinalIgnoreCase)).Value;

                            if(known != null)
                                _voqooeCartoData.TryRemove(known.Address, out var value);
                        }
                    }
                    break;
                case JournalTypeEnum.MultiSellExplorationData:
                    if (e.CommanderID == _currentCommander.Id
                       && e.EventData is MultiSellExplorationDataEvent.MultiSellExplorationDataEventArgs multiSellCarto)
                    {
                        foreach (var system in multiSellCarto.Discovered)
                        {
                            var known = _voqooeCartoData.FirstOrDefault(x => x.Value.Name.Equals(system.SystemName, StringComparison.OrdinalIgnoreCase)).Value;

                            if (known != null)
                                _voqooeCartoData.TryRemove(known.Address, out var value);
                        }
                    }
                    break;
                case JournalTypeEnum.Touchdown:
                    if (e.CommanderID == _currentCommander.Id
                        && e.EventData is TouchdownEvent.TouchdownEventArgs touchdown)
                    {
                        CurrentBodyName = touchdown.Body;
                    }
                    break;
                case JournalTypeEnum.ScanOrganic:
                    if (e.CommanderID == _currentCommander.Id
                        && e.EventData is ScanOrganicEvent.ScanOrganicEventArgs scanOrganic
                        && scanOrganic.ScanType.Equals("Analyse"))
                    {
                        _ = AddBioData(scanOrganic.Species, scanOrganic.Species_Localised, scanOrganic.Genus, scanOrganic.Genus_Localised, OrganicScanState.Analysed);
                        _organicDataToSell.Add(new(scanOrganic.Variant_Localised ?? scanOrganic.Species_Localised, CurrentBodyName, scanOrganic.Species_Localised, scanOrganic.Species));

                        if (Ready)
                        {
                            OnOrganicToSellDataChanged?.Invoke(this, System.EventArgs.Empty);
                        }
                    }
                    break;
                case JournalTypeEnum.SellOrganicData:
                    if (e.CommanderID == _currentCommander.Id
                        && e.EventData is SellOrganicDataEvent.SellOrganicDataEventArgs sellOrganic)
                    {
                        var ret = new List<OrganicScanDetails>();
                        var itemsSold = false;
                        foreach (var organic in sellOrganic.BioData)
                        {
                            ret.Add(AddBioData(organic.Species, organic.Species_Localised, organic.Genus, organic.Genus_Localised, OrganicScanState.Sold, false));

                            var haveToSell = _organicDataToSell.FirstOrDefault(x => x.Species_Local.Equals(organic.Species_Localised, StringComparison.OrdinalIgnoreCase));

                            if (haveToSell != null)
                            {
                                itemsSold = true;
                                _organicDataToSell.Remove(haveToSell);
                            }
                        }

                        if (Ready == false)
                        {
                            break;
                        }
                        if (itemsSold)
                        {
                            OnOrganicToSellDataChanged?.Invoke(this, System.EventArgs.Empty);
                        }

                        OnOrganicDataChanged?.Invoke(this, ret);
                    }
                    break;
                case JournalTypeEnum.Died:
                    if (e.CommanderID == _currentCommander.Id
                        && e.EventData is DiedEvent.DiedEventArgs diedEvent)
                    {
                        var itemsToRemove = bioData.Where(x => x.Value.State != OrganicScanState.Sold);

                        foreach (var item in itemsToRemove)
                        {
                            bioData.Remove(item.Key);
                        }

                        _organicDataToSell.Clear();

                        if (Ready == false)
                        {
                            break;
                        }
                        OnOrganicToSellDataChanged?.Invoke(this, System.EventArgs.Empty);
                        OnOrganicDataChanged?.Invoke(this, BioData.Values);
                    }
                    break;

            }
        }

        private OrganicScanDetails AddBioData(string species, string species_localised, string genus, string genus_localised, OrganicScanState scanState, bool fireEvent = true)
        {
            if (bioData.ContainsKey(species) == false)
            {
                bioData.TryAdd(species, new(species, species_localised, genus, genus_localised));
            }

            var data = bioData[species];

            if (data.State != OrganicScanState.Sold)
                data.State = scanState;

            if (fireEvent && Ready)
            {
                OnOrganicDataChanged?.Invoke(this, [data]);
            }

            return data;
        }
        public async Task Load()
        {
            try
            {
                await _initializeLazy.Value;
            }
            catch (Exception ex)
            {
                loggerStore.LogError("Voqooe Datastore", ex);
                _ = ODMessageBox.Show(null, "Error starting datastore", $"See VPLog.txt in {App.BaseDirectory} for details");
                _initializeLazy = new Lazy<Task>(Initialise);
                throw;
            }
        }

        private async Task Initialise()
        {
#if DEBUG
            //await voqooeDatabaseProvider.ResetDataBaseAsync();
#endif
            HubSystem = voqooeDatabaseProvider.GetVoqooeSystemByName("Voqooe BI-H d11-864");

            await UpdateCommanders();

            Loaded = true;

            //First run
            if (_journalCommanders.Count == 0)
            {
                PerformFirstRun();
                return;
            }
            var cmdr = GetSelectedComander(settingsStore.SelectedCommanderID);
            await SetSelectedCommander(cmdr);
        }

        public void PerformFirstRun()
        {
            FirstRun = true;
            journalWatcherStore.StopWatcher();
            journalWatcherStore.StartWatching(new(0, "", "", "", false));
        }

        private async Task FinishFirstRun()
        {

            await UpdateCommanders();
            var cmdr = GetSelectedComander(0);
            await SetSelectedCommander(cmdr);
            FirstRun = false;
        }

        public void ChangeCommander(int id)
        {
            if (Ready == false)
                return;

            _ = Task.Run(async () =>
            {
                bioData.Clear();
                _organicDataToSell.Clear();
                Ready = false;
                ReadyStatusChange?.Invoke(this, Ready);
                var cmdr = GetSelectedComander(id);
                await SetSelectedCommander(cmdr);
            });

        }

        public async Task ScanNewDirctory(string dir)
        {
            Ready = false;
            ReadyStatusChange?.Invoke(this, Ready);

            _currentCommander = new JournalCommander(0, string.Empty, dir, string.Empty, false);
            await SetSelectedCommander(_currentCommander);
        }

        public void UpdateNearByOptions()
        {
            Task.Run(UpdateSystems);
        }

        private async Task UpdateSystems()
        {
            if (CurrentSystem == null)
                return;

            var systems = await voqooeDatabaseProvider.GetNearestXSystems(CurrentSystem, 50, settingsStore.NearBySystemsOptions, _currentCommander.Id, settingsStore.StarClassFilter);
            _voqooeSystems.Clear();
            foreach (var system in systems)
            {
                var distance = SystemPosition.Distance(currentSystem?.Pos ?? new(), new SystemPosition() { X = system.X, Y = system.Y, Z = system.Z });
                var beenThere = voqooeDatabaseProvider.HasCommanderVisitedSystem(_currentCommander.Id, system.Address);
                system.UserVisited = beenThere;
                system.Distance = distance;
            }
            _voqooeSystems.AddRange(systems);
            OnSystemsUpdated?.Invoke(this, System.EventArgs.Empty);
        }

        private async Task SetSelectedCommander(JournalCommander commander)
        {
            journalWatcherStore.StopWatcher();
            _currentCommander = commander;
            _systemVisitsToAdd.Clear();

            if (_currentCommander.Id != 0)
                settingsStore.SelectedCommanderID = _currentCommander.Id;

            if (Loaded)
            {
                OnCurrentCommanderChanged?.Invoke(this, _currentCommander);
            }

            await BuildExplorationData();
            journalWatcherStore.StartWatching(commander);
        }

        private async Task BuildExplorationData()
        {
            bioData.Clear();
            _voqooeCartoData.Clear();

            //exo events
            var events = await voqooeDatabaseProvider.GetJournalEntriesOfType(_currentCommander.Id,
                [(int)JournalTypeEnum.ScanOrganic, (int)JournalTypeEnum.SellOrganicData, (int)JournalTypeEnum.Touchdown]);            

            foreach (var e in events)
            {
                ProcessJournalEntry(e);
            }

            //carto data for the last 30 days to reduce memory useage
            //TODO add parameter in settings for age
            events = await voqooeDatabaseProvider.GetJournalEntriesOfType(_currentCommander.Id,
                       [(int)JournalTypeEnum.FSSDiscoveryScan, (int)JournalTypeEnum.Scan, (int)JournalTypeEnum.SAAScanComplete,
                    (int)JournalTypeEnum.SellExplorationData, (int)JournalTypeEnum.MultiSellExplorationData,
                    (int)JournalTypeEnum.FSDJump, (int)JournalTypeEnum.Location, (int)JournalTypeEnum.Died], DateTime.Now.AddDays(-30));

            foreach (var e in events)
            {
                ProcessJournalEntry(e);
            }
        }

        private JournalCommander GetSelectedComander(int id)
        {
            var ret = _journalCommanders.FirstOrDefault(x => x.Id == id);

            if (ret == null && _journalCommanders.Count != 0)
            {
                ret = _journalCommanders.FirstOrDefault();
            }

            return ret ?? new JournalCommander(0, string.Empty, string.Empty, string.Empty, false);
        }

        public async Task UpdateCommanders()
        {
            var cmdrs = await voqooeDatabaseProvider.GetAllJournalCommanders();
            _journalCommanders.Clear();
            _journalCommanders.AddRange(cmdrs);

            //If we haven't found any commanders yet, set the first one
            if (settingsStore.SelectedCommanderID == 0 && _journalCommanders.Count != 0)
            {
                _currentCommander = _journalCommanders.First();
                settingsStore.SelectedCommanderID = _currentCommander.Id;
            }
            if (Loaded)
            {
                OnCommandersUpdated?.Invoke(this, System.EventArgs.Empty);
            }
        }
    }
}
