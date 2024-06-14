using EliteJournalReader;
using EliteJournalReader.Events;
using ODUtils.Dialogs;
using System.Collections.Concurrent;
using System.Windows;
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

        private readonly Dictionary<string, OrganicScanDetails> bioData;
        public Dictionary<string, OrganicScanDetails> BioData => bioData;

        private readonly List<OrganicDetails> _organicDataToSell;
        public IEnumerable<OrganicDetails> OrganicDataToSell => _organicDataToSell;

        public bool Loaded { get; private set; }
        public bool Ready { get; private set; }

        #region Events
        public EventHandler? OnCommandersUpdated;
        public EventHandler<JournalCommander>? OnCurrentCommanderChanged;
        public EventHandler<bool>? ReadyStatusChange;
        public EventHandler? OnSystemsUpdated;
        public EventHandler<JournalSystem?>? OnCurrentSystemChanged;
        public EventHandler<IEnumerable<OrganicScanDetails>>? OnOrganicDataChanged;
        public EventHandler? OnOrganicToSellDataChanged;
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
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_systemVisitsToAdd.Count != 0)
                {
                    foreach (var system in _systemVisitsToAdd)
                    {
                        voqooeDatabaseProvider.AddCommanderVisits(system.Value, system.Key);
                    }
                    _systemVisitsToAdd.Clear();
                }

                _ = Task.Run(UpdateSystems);

                OnCurrentSystemChanged?.Invoke(this, currentSystem);
                OnCurrentCommanderChanged?.Invoke(this, _currentCommander);
                Ready = e && Loaded;
                ReadyStatusChange?.Invoke(this, Ready);
            });
        }

        private void OnJournalEntry(object? sender, JournalEntry e)
        {
            if (e.EventType == JournalTypeEnum.Commander)
            {
                var task = Task.Run(UpdateCommanders);
                task.Wait();
                return;
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
                        if (fsdJump.StarSystem.StartsWith("Voqooe") == false)
                        {
                            break;
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
                            OnOrganicToSellDataChanged?.Invoke(this, EventArgs.Empty);
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
                            OnOrganicToSellDataChanged?.Invoke(this, EventArgs.Empty);
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
                        OnOrganicToSellDataChanged?.Invoke(this, EventArgs.Empty);
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
            HubSystem = voqooeDatabaseProvider.GetVoqooeSystemByName("Voqooe BI-H d11-864");

            await UpdateCommanders();

            Loaded = true;

            var cmdr = GetSelectedComander(settingsStore.SelectedCommanderID);
            await SetSelectedCommander(cmdr);

            OnCommandersUpdated?.Invoke(this, EventArgs.Empty);
            OnCurrentCommanderChanged?.Invoke(this, _currentCommander);
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
            OnSystemsUpdated?.Invoke(this, EventArgs.Empty);
        }

        private async Task SetSelectedCommander(JournalCommander commander)
        {
            journalWatcherStore.StopWatcher();
            _currentCommander = commander;
            _systemVisitsToAdd.Clear();

            if(_currentCommander.Id != 0)
                settingsStore.SelectedCommanderID = _currentCommander.Id;

            if (Loaded)
            {
                OnCurrentCommanderChanged?.Invoke(this, _currentCommander);
            }

            await BuildBioData();
            journalWatcherStore.StartWatching(commander);
        }

        private async Task BuildBioData()
        {
            bioData.Clear();
            var events = await voqooeDatabaseProvider.GetJournalEntriesOfType(_currentCommander.Id, [(int)JournalTypeEnum.ScanOrganic, (int)JournalTypeEnum.SellOrganicData, (int)JournalTypeEnum.Died, (int)JournalTypeEnum.Touchdown]);

            foreach (var e in events)
            {
                ProcessJournalEntry(e);
            }
        }

        private JournalCommander GetSelectedComander(int id)
        {
            var ret = _journalCommanders.FirstOrDefault(x => x.Id == id);

            if (ret == null && _journalCommanders.Any())
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
                OnCommandersUpdated?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
