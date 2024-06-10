using EliteJournalReader;
using Microsoft.EntityFrameworkCore;
using ODUtils.Dialogs;
using VoqooePlanner.DbContexts;
using VoqooePlanner.DTOs;
using VoqooePlanner.Models;

namespace VoqooePlanner.Services.Database
{
    public class VoqooeDatabaseProvider(IVoqooeDbContextFactory voqooeDbContextFactory) : IVoqooeDatabaseProvider
    {
        private readonly IVoqooeDbContextFactory voqooeDbContextFactory = voqooeDbContextFactory;

        #region System Methods
        public async Task<int> UpdateVoqooeSystems(IEnumerable<VoqooeSystemDTO> voqooeSystems)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            try
            {
                var ret = await context.Systems
                    .UpsertRange(voqooeSystems)
                    .On(s => s.Address)
                    .UpdateIf((matched, newItem) => matched.Value != newItem.Value
                                                 && matched.Visited != newItem.Visited
                                                 && matched.ContainsELW != newItem.ContainsELW
                                                 && matched.StarType != newItem.StarType)
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
                _= ODMessageBox.Show(null, "Error Updating Voqooe Systems", ex.Message);
                return 0;
            }
        }

        public void AddCommanderVisit(VoqooeSystem voqooeSystem, int cmdrId)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            var cmdr = context.JournalCommanders.First(x => x.Id == cmdrId);

            var system = context.Systems.Include(x => x.CommanderVisits).FirstOrDefault(x => x.Address == voqooeSystem.Address);

            if(system == null) 
            {
                UpdateVoqooeSystem(voqooeSystem);
                system = context.Systems.Include(x => x.CommanderVisits).FirstOrDefault(x => x.Address == voqooeSystem.Address);
                if(system == null ) 
                {
                    return;
                }
            }
            if(system.CommanderVisits.Contains(cmdr) == false)
            {
                system.CommanderVisits.Add(cmdr);
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

                    if (known is null)
                    {
                        UpdateVoqooeSystem(system);
                        known = systems.First(x => x.Address == system.Address);
                    }

                    if (known is not null && known.CommanderVisits.Contains(cmdr) == false)
                    {
                        known.CommanderVisits.Add(cmdr);
                    }
                }

                context.SaveChanges();
            }
            catch (Exception ex) 
            {
                _ = ODMessageBox.Show(null, "Error Updating CMDR System Visits", ex.Message);
            }
        }

        public void UpdateVoqooeSystem(VoqooeSystem voqooeSystem)
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
            if(dto != null) 
            {
                ret = new(dto);
            }
            return ret;
        }

        public bool HasCommanderVisitedSystem(int id, long address)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            var cmdr = context.JournalCommanders.First(x => x.Id == id);

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

            if (includeHidden)
            {
                return await context.JournalCommanders
                    .Select(x => new JournalCommander(x.Id, x.Name, x.JournalDir, x.LastFile, x.IsHidden))
                    .ToListAsync();
            }

            return await context.JournalCommanders
                .Where(x => x.IsHidden == false)
                .Select(x => new JournalCommander(x.Id, x.Name, x.JournalDir, x.LastFile, x.IsHidden))
                    .ToListAsync();
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
                .OrderBy(x => x.Filename)
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

        public async Task<IEnumerable<JournalEntry>> GetJournalEntriesOfType(int cmdrId, IEnumerable<int> types)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            var ret = await context.JournalEntries
                .EventTypeCompare(cmdrId, types)
                //.OrderBy(x => x.Filename)
                //.ThenBy(x => x.Offset)
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

        public void AddJournalEntries(IEnumerable<JournalEntry> journalEntries)
        {
            var entriesToAdd = journalEntries
                .OrderBy(x => x.Filename)
                .ThenBy(x => x.Offset)
                .Select(x => new JournalEntryDTO()
                {
                    CommanderID = x.CommanderID,
                    EventData = x.OriginalEvent?.ToString(Newtonsoft.Json.Formatting.None) ?? string.Empty,
                    EventTypeId = (int)x.EventType,
                    Filename = x.Filename,
                    Offset = x.Offset
                }
                );

            using var context = voqooeDbContextFactory.CreateDbContext();

            context.JournalEntries
                .UpsertRange(entriesToAdd)
                .On(e => new { e.Filename, e.Offset })
                .Run();
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
        }

        public void AddSetting(SettingsDTO settings)
        {
            using var context = voqooeDbContextFactory.CreateDbContext();

            context.Settings.
                Upsert(settings)
                .On(x => x.Id)
                .Run();
        }
        #endregion
    }
}
