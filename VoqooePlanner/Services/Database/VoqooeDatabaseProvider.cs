using EliteJournalReader;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ODUtils.Dialogs;
using VoqooePlanner.DbContexts;
using VoqooePlanner.DTOs;
using VoqooePlanner.Models;
using VoqooePlanner.Stores;

namespace VoqooePlanner.Services.Database
{
    public class VoqooeDatabaseProvider(IVoqooeDbContextFactory voqooeDbContextFactory, LoggerStore loggerStore) : IVoqooeDatabaseProvider
    {
        private readonly IVoqooeDbContextFactory voqooeDbContextFactory = voqooeDbContextFactory;
        private readonly LoggerStore loggerStore = loggerStore;

        #region System Methods
        public async Task<int> UpdateVoqooeSystems(IEnumerable<VoqooeSystemDTO> voqooeSystems)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            try
            {
                var ret = await context.Systems
                    .UpsertRange(voqooeSystems)
                    .On(s => s.Address)
                    .UpdateIf((matched, newItem) => matched.Value < newItem.Value
                                                 || matched.Visited != newItem.Visited
                                                 || matched.ContainsELW != newItem.ContainsELW
                                                 || matched.StarType != newItem.StarType)
                    .WhenMatched((matched, newItem) => new VoqooeSystemDTO()
                    {
                        Visited = newItem.Visited,
                        ContainsELW = newItem.ContainsELW,
                        Value = newItem.Value,
                        StarType = newItem.StarType,
                    })
                    .RunAsync();

                return ret;
            }
            catch (Exception ex)
            {
                loggerStore.LogError("DataBase Provider", ex);
                _ = ODMessageBox.Show(null, "Error Updating Voqooe Systems", $"See VPLog.txt in {App.BaseDirectory} for details");
                return 0;
            }
        }

        public void AddCommanderVisit(VoqooeSystem voqooeSystem, int cmdrId)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            var cmdr = context.JournalCommanders.First(x => x.Id == cmdrId);

            var system = context.Systems.Include(x => x.CommanderVisits).FirstOrDefault(x => x.Address == voqooeSystem.Address);

            if (system == null)
            {
                UpdateVoqooeSystem(voqooeSystem);
                system = context.Systems.Include(x => x.CommanderVisits).FirstOrDefault(x => x.Address == voqooeSystem.Address);
                if (system == null)
                {
                    return;
                }
            }
            if (system.CommanderVisits.Contains(cmdr) == false)
            {
                system.CommanderVisits.Add(cmdr);
                system.Visited = true;
                context.SaveChanges();
            }

        }

        public void AddCommanderVisits(IEnumerable<VoqooeSystem> voqooeSystems, int cmdrId)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            try
            {
                var cmdr = context.JournalCommanders.First(x => x.Id == cmdrId);

                var systems = context.Systems.Include(x => x.CommanderVisits).ToList();

                foreach (var system in voqooeSystems)
                {
                    var known = systems.FirstOrDefault(x => x.Address == system.Address);

                    known ??= UpdateVoqooeSystem(system);

                    if (known is not null && known.CommanderVisits.Contains(cmdr) == false)
                    {
                        known.CommanderVisits.Add(cmdr);
                    }
                }

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                loggerStore.LogError("DataBase Provider", ex);
                _ = ODMessageBox.Show(null, "Error Updating CMDR System Visits", $"See VPLog.txt in {App.BaseDirectory} for details");
            }
        }

        public VoqooeSystemDTO UpdateVoqooeSystem(VoqooeSystem voqooeSystem)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            var dto = new VoqooeSystemDTO()
            {
                Address = voqooeSystem.Address,
                Name = voqooeSystem.Name,
                StarType = voqooeSystem.StarType,
                Visited = voqooeSystem.Visited,
                X = voqooeSystem.X,
                Y = voqooeSystem.Y,
                Z = voqooeSystem.Z,
            };
            context.Systems
                .Upsert(dto)
                .On(x => x.Address)
                .WhenMatched((matched, newSystem) => new VoqooeSystemDTO()
                {
                    Visited = newSystem.Visited,
                    StarType = newSystem.StarType,
                })
                .Run();

            context.SaveChanges();
            return dto;
        }

        public void UpdateVoqooeSystems(IEnumerable<VoqooeSystem> voqooeSystem)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            var dtos = voqooeSystem.Select(x => new VoqooeSystemDTO()
            {
                Address = x.Address,
                Name = x.Name,
                StarType = x.StarType,
                Visited = x.Visited,
                X = x.X,
                Y = x.Y,
                Z = x.Z,
            });
            context.Systems
                .UpsertRange(dtos)
                .On(x => x.Address)
                .WhenMatched((matched, newSystem) => new VoqooeSystemDTO()
                {
                    Visited = newSystem.Visited,
                    StarType = newSystem.StarType,
                })
                .Run();

            context.SaveChanges();
        }

        public async Task<IEnumerable<VoqooeSystem>> GetAllVoqooeSystems()
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            return await context.Systems.Select(x => new VoqooeSystem(x)).ToListAsync();
        }

        public VoqooeSystem? GetVoqooeSystemByName(string name)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            VoqooeSystem? ret = null;
            var dto = context.Systems.FirstOrDefault(x => x.Name == name);
            if (dto != null)
            {
                ret = new(dto);
            }
            return ret;
        }

        public bool HasCommanderVisitedSystem(int id, long address)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            var cmdr = context.JournalCommanders.FirstOrDefault(x => x.Id == id);

            if (cmdr == null) { return false; }

            var system = context.Systems.Include(x => x.CommanderVisits).FirstOrDefault(x => x.Address == address);

            if (system is not null && system.CommanderVisits.Contains(cmdr))
            {
                return true;
            }
            return false;
        }

        public async Task<List<VoqooeSystem>> GetNearestXSystems(JournalSystem system, int count, NearBySystemsOptions options, int commanderId, IEnumerable<int> starfilters)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            var ret = context.Systems
                .Where(x => x.Address != system.Address)
                .CheckWhatToInclude(options, commanderId)
                .FilterStars(starfilters)
                .Select(s => new
                {
                    dto = s,
                    distance = (float)Math.Sqrt((double)(s.X - system.Pos.X) * (double)(s.X - system.Pos.X)
                    + (double)(s.Y - system.Pos.Y) * (double)(s.Y - system.Pos.Y)
                    + (double)(s.Z - system.Pos.Z) * (double)(s.Z - system.Pos.Z))
                })
                .OrderBy(s => s.distance)
                .Take(count)
                .Select(x => new VoqooeSystem(x.dto));

            return await ret.ToListAsync();
        }
        #endregion

        #region Commander Methods
        public async Task<IEnumerable<JournalCommander>> GetAllJournalCommanders(bool includeHidden = false)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            if(!context.JournalCommanders.Any())
                return [];

            if (includeHidden)
            {
                var allCmdrs = await context.JournalCommanders
                    .Select(x => new JournalCommander(x.Id, x.Name, x.JournalDir, x.LastFile, x.IsHidden))                    
                    .ToListAsync();

                var reslt = allCmdrs.OrderBy(x => x.Name.Contains("(Legacy)"))
                                    .ThenBy(x => x.Name);
                return reslt;
            }

            var cmdrs = await context.JournalCommanders
                .Where(x => x.IsHidden == false)
                .Select(x => new JournalCommander(x.Id, x.Name, x.JournalDir, x.LastFile, x.IsHidden))
                .ToListAsync();

            var ret = cmdrs.OrderBy(x => x.Name.Contains("(Legacy)"))
                            .ThenBy(x => x.Name);
            return ret;
        }

        public JournalCommander AddCommander(JournalCommander cmdr)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            var known = context.JournalCommanders.FirstOrDefault(x => x.Name == cmdr.Name);

            if (known == null)
            {
                known = new JournalCommanderDTO()
                {
                    Name = cmdr.Name,
                    LastFile = cmdr.LastFile ?? string.Empty,
                    JournalDir = cmdr.JournalPath ?? string.Empty,
                    IsHidden = cmdr.IsHidden
                };
                context.JournalCommanders.Add(known);
                context.SaveChanges();
                return new(known.Id, known.Name, known.JournalDir, known.LastFile, known.IsHidden);
            }

            known.LastFile = cmdr.LastFile ?? string.Empty;
            known.JournalDir = cmdr.JournalPath ?? string.Empty;
            known.Name = cmdr.Name;
            known.IsHidden = cmdr.IsHidden;
            context.SaveChanges();
            return new(known.Id, known.Name, known.JournalDir, known.LastFile, known.IsHidden);
        }
        #endregion

        #region Journal Entry Methods
        public async Task<IEnumerable<JournalEntry>> GetAllJournalEntries(int cmdrId)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            var ret = await context.JournalEntries
                .Where(x => x.CommanderID == cmdrId)
                .OrderBy(x => x.TimeStamp)
                .ThenBy(x => x.Offset)
                .Select(x => new JournalEntry(
                    x.Filename,
                    x.Offset,
                    x.CommanderID,
                    (JournalTypeEnum)x.EventTypeId,
                    JournalWatcher.GetEventData(x.EventData),
                    null))
                .ToListAsync();

            return ret;
        }

        public async Task<IEnumerable<JournalEntry>> GetJournalEntriesOfType(int cmdrId, IEnumerable<JournalTypeEnum> types)
        {
            return await GetJournalEntriesOfType(cmdrId, types, DateTime.MinValue);
        }

        public async Task<IEnumerable<JournalEntry>> GetJournalEntriesOfType(int cmdrId, IEnumerable<JournalTypeEnum> types, DateTime age)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            var ret = await context.JournalEntries
                .Where(x => x.TimeStamp >= age)
                .EventTypeCompare(cmdrId, types)
                .OrderBy(x => x.TimeStamp)
                .ThenBy(x => x.Offset)
                .Select(x => new JournalEntry(
                    x.Filename,
                    x.Offset,
                    x.CommanderID,
                    (JournalTypeEnum)x.EventTypeId,
                    JournalWatcher.GetEventData(x.EventData),
                    null))
                .ToListAsync();

            return ret;
        }

        public Task ParseJounralEventsOfType(int cmdrId, IEnumerable<JournalTypeEnum> types, Action<JournalEntry> method, DateTime age)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            var entries = context.JournalEntries
                .Where(x => x.TimeStamp >= age)
                .EventTypeCompare(cmdrId, types)
                .OrderBy(x => x.TimeStamp)
                .ThenBy(x => x.Offset);

            foreach (var e in entries)
            {                
                method.Invoke(new JournalEntry(
                    e.Filename,
                    e.Offset,
                    e.CommanderID,
                    (JournalTypeEnum)e.EventTypeId,
                    JournalWatcher.GetEventData(e.EventData),
                    null));
            }
            return Task.CompletedTask;
        }
        public void AddJournalEntries(IEnumerable<JournalEntry> journalEntries)
        {
            var entriesToAdd = journalEntries
                .OrderBy(x => x.Filename)
                .ThenBy(x => x.Offset)
                .Select(x => new JournalEntryDTO()
                {
                    CommanderID = x.CommanderID,
                    EventData = x.OriginalEvent?.ToString(Formatting.None) ?? string.Empty,
                    EventTypeId = (int)x.EventType,
                    Filename = x.Filename,
                    Offset = x.Offset,
                    TimeStamp = x.TimeStamp,
                }
                );

            using var context = voqooeDbContextFactory.CreateDbContext();

            context.JournalEntries
                .UpsertRange(entriesToAdd)
                .On(e => new { e.Filename, e.Offset })
                .Run();

            context.SaveChanges();
        }
        #endregion

        #region Settings Methods
        public IEnumerable<SettingsDTO> GetAllSettings()
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            return context.Settings.ToList();
        }

        public void AddSettings(IEnumerable<SettingsDTO> settings)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            context.Settings.
                UpsertRange(settings)
                .On(x => x.Id)
                .Run();

            context.SaveChanges();
        }

        public void AddSetting(SettingsDTO settings)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            context.Settings.
                Upsert(settings)
                .On(x => x.Id)
                .Run();

            context.SaveChanges();
        }
        #endregion

        #region Database Methods
        public Task ResetDataBaseAsync()
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            context.Database.ExecuteSqlRawAsync(
                "DELETE FROM CommanderVistedSystems;" +
                "DELETE FROM JournalCommanders;" +
                "DELETE FROM JournalEntries;" +
                "DELETE FROM CommanderIgnoredSystems;" +
                "DELETE FROM CartoIgnoredSystems;" +
                "DELETE FROM SQLITE_SEQUENCE WHERE name='CommanderVistedSystems';" +
                "DELETE FROM SQLITE_SEQUENCE WHERE name='JournalCommanders';" +
                "DELETE FROM SQLITE_SEQUENCE WHERE name='CommanderIgnoredSystems';" +
                "DELETE FROM SQLITE_SEQUENCE WHERE name='CartoIgnoredSystems';" +
                "DELETE FROM SQLITE_SEQUENCE WHERE name='JournalEntries';");

            context.SaveChangesAsync();

            return Task.CompletedTask;
        }

        public Task AddIgnoreSystem(long address, string name, int cmdrId)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            var cmdr = context.JournalCommanders.First(x => x.Id == cmdrId);

            var system = context.CartoIgnoredSystems.Include(x => x.Commanders).FirstOrDefault(x => x.Address == address);

            if (system == null)
            {
                context.CartoIgnoredSystems.Add(new CartoIgnoredSystemsDTO
                {
                    Address = address,
                    Name = name,
                    Commanders = [cmdr]
                });
                context.SaveChanges(true); 
                return Task.CompletedTask;
            }
            if (system.Commanders.Contains(cmdr) == false)
            {
                system.Commanders.Add(cmdr);
                context.SaveChanges(true);
            }
            return Task.CompletedTask;
        }

        public Task RemoveIgnoreSystem(long address, int cmdrId)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            var cmdr = context.JournalCommanders.First(x => x.Id == cmdrId);

            var system = context.CartoIgnoredSystems.Include(x => x.Commanders).FirstOrDefault(x => x.Address == address && x.Commanders.Contains(cmdr));

            if(system != null)
            {
                system.Commanders.Remove(cmdr);
                context.SaveChanges();
            }

            return Task.CompletedTask;
        }

        public Dictionary<long, string> GetIgnoredSytemsDictionary(int cmdrId)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            var cmdr = context.JournalCommanders.First(x => x.Id == cmdrId);

            var systems = context.CartoIgnoredSystems
                .Include(x => x.Commanders)
                .Where(x => x.Commanders.Contains(cmdr));

            return systems.ToDictionary(x => x.Address, x => x.Name);
        }

        public IEnumerable<IgnoredSystem> GetIgnoredSytems(int cmdrId)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            var cmdr = context.JournalCommanders.First(x => x.Id == cmdrId);

            var ret = context.CartoIgnoredSystems
                .Include(x => x.Commanders)
                .Where(x => x.Commanders.Contains(cmdr))
                .OrderBy(x => x.Name)
                .Select(x => new IgnoredSystem(
                    x.Address,
                    x.Name,
                    cmdrId))
                    .ToList();

            return ret;
        }
        #endregion
    }
}
