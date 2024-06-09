using EliteJournalReader;
using EliteJournalReader.Events;
using System.Collections.Concurrent;
using VoqooePlanner.Models;
using VoqooePlanner.Services.Database;

namespace VoqooePlanner.Stores
{
    public sealed class VoqooeDataStore
    {
        private readonly IVoqooeDatabaseProvider voqooeDatabaseProvider;
        private readonly JournalWatcherStore journalWatcherStore;
        private readonly SettingsStore settingsStore;
        private Lazy<Task> _initializeLazy;

        private readonly List<VoqooeSystem> _voqooeSystems;
        private List<JournalCommander> _journalCommanders;
        private readonly ConcurrentDictionary<int, List<VoqooeSystem>> _systemVisitsToAdd = [];
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
        public VoqooeSystem? HubSystem { get; private set; }
        public IEnumerable<VoqooeSystem> VoqooeSystems => _voqooeSystems;
        public IEnumerable<JournalCommander> JournalCommanders => _journalCommanders;
        public bool Loaded { get; private set; }
        public bool Ready { get; private set; }
        public EventHandler? OnCommandersUpdated;
        public EventHandler<JournalCommander>? OnCurrentCommanderChanged;
        public EventHandler<bool>? ReadyStatusChange;
        public EventHandler? OnSystemsUpdated;
        public EventHandler<JournalSystem?>? OnCurrentSystemChanged;

        public VoqooeDataStore(IVoqooeDatabaseProvider voqooeDatabaseProvider, JournalWatcherStore journalWatcherStore, SettingsStore settingsStore)
        {
            this.voqooeDatabaseProvider = voqooeDatabaseProvider;
            this.journalWatcherStore = journalWatcherStore;
            this.settingsStore = settingsStore;

            _initializeLazy = new Lazy<Task>(Initialise);
            _voqooeSystems = [];
            _journalCommanders = [];
            journalWatcherStore.OnJournalEventRecieved += OnJournalEntry;
            journalWatcherStore.LiveStatusChange += OnLiveStatusChange;
        }

        private void OnLiveStatusChange(object? sender, bool e)
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
            }
        }

        public async Task Load()
        {
            try
            {
                await _initializeLazy.Value;
            }
            catch (Exception)
            {
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
            SetSelectedCommander(cmdr);

            OnCommandersUpdated?.Invoke(this, EventArgs.Empty);
            OnCurrentCommanderChanged?.Invoke(this, _currentCommander);
        }

        public void ChangeCommander(int id)
        {
            if (Ready == false)
                return;

            _ = Task.Run(() =>
            {
                Ready = false;
                ReadyStatusChange?.Invoke(this, Ready);
                var cmdr = GetSelectedComander(id);
                SetSelectedCommander(cmdr);
            });

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
        private void SetSelectedCommander(JournalCommander commander)
        {
            journalWatcherStore.StopWatcher();
            _currentCommander = commander;
            _systemVisitsToAdd.Clear();
            settingsStore.SelectedCommanderID = _currentCommander.Id;

            if (Loaded)
            {
                OnCurrentCommanderChanged?.Invoke(this, _currentCommander);
            }
            journalWatcherStore.StartWatching(commander);
        }

        private JournalCommander GetSelectedComander(int id)
        {
            var ret = _journalCommanders.FirstOrDefault(x => x.Id == id) ?? new JournalCommander(0, string.Empty, string.Empty, string.Empty, false);
            return ret;
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
