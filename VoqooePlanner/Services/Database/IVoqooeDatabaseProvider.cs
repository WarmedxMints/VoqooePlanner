using EliteJournalReader;
using VoqooePlanner.DTOs;
using VoqooePlanner.Models;

namespace VoqooePlanner.Services.Database
{
    public interface IVoqooeDatabaseProvider
    {
        public Task<int> UpdateVoqooeSystems(IEnumerable<VoqooeSystemDTO> voqooeSystems);
        public VoqooeSystemDTO UpdateVoqooeSystem(VoqooeSystem voqooeSystem);
        public void UpdateVoqooeSystems(IEnumerable<VoqooeSystem> voqooeSystem);
        public Task<IEnumerable<VoqooeSystem>> GetAllVoqooeSystems();
        public void AddCommanderVisit(VoqooeSystem voqooeSystem, int cmdrId);
        public void AddCommanderVisits(IEnumerable<VoqooeSystem> voqooeSystems, int cmdrId);
        public VoqooeSystem? GetVoqooeSystemByName(string name);
        public Task<List<VoqooeSystem>> GetNearestXSystems(JournalSystem system, int count, NearBySystemsOptions options, int commanderId, IEnumerable<int> starfilters);

        public bool HasCommanderVisitedSystem(int id, long address);

        public Task<IEnumerable<JournalCommander>> GetAllJournalCommanders(bool includeHidden = false);
        public JournalCommander AddCommander(JournalCommander commander);

        public Task<IEnumerable<JournalEntry>> GetAllJournalEntries(int cmdrId);            
        public Task<IEnumerable<JournalEntry>> GetJournalEntriesOfType(int cmdrId, IEnumerable<JournalTypeEnum> types);
        public Task<IEnumerable<JournalEntry>> GetJournalEntriesOfType(int cmdrId, IEnumerable<JournalTypeEnum> types, DateTime age);
        public Task ParseJounralEventsOfType(int cmdrId, IEnumerable<JournalTypeEnum> type, Action<JournalEntry> method, DateTime age);
        public void AddJournalEntries(IEnumerable<JournalEntry> journalEntries);

        public IEnumerable<SettingsDTO> GetAllSettings();
        public void AddSettings(IEnumerable<SettingsDTO> settings);
        public void AddSetting(SettingsDTO settings);

        public Task ResetDataBaseAsync();
        public Task AddIgnoreSystem(long address, string name, int cmdrId);
        public Task RemoveIgnoreSystem(long address, int cmdrId);
        public Dictionary<long, string> GetIgnoredSytemsDictionary(int cmdrId);
        public IEnumerable<IgnoredSystem> GetIgnoredSytems(int cmdrId);

    }
}
