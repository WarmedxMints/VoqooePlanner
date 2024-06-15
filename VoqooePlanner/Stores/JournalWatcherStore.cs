using EliteJournalReader.Events;
using EliteJournalReader;
using VoqooePlanner.Models;
using VoqooePlanner.Services.Database;
using System.Collections.Concurrent;
using ODUtils.EliteDangerousHelpers;
using Newtonsoft.Json.Linq;
using System.IO;
using ODUtils.Dialogs;

namespace VoqooePlanner.Stores
{
    public sealed class JournalWatcherStore(SettingsStore settingsStore, IVoqooeDatabaseProvider voqooeDatabaseProvider, LoggerStore loggerStore)
    {
        private readonly SettingsStore settingsStore = settingsStore;
        private readonly IVoqooeDatabaseProvider voqooeDatabaseProvider = voqooeDatabaseProvider;
        private readonly LoggerStore loggerStore = loggerStore;
        private JournalWatcher? watcher;
        private JournalCommander? journalCommander;
        private readonly ConcurrentDictionary<Tuple<string, long>, JournalEntry> _messagesReceived = [];
        private bool legacy;
        public EventHandler<bool>? LiveStatusChange;
        public EventHandler<JournalEntry>? OnJournalEventRecieved;
        public EventHandler<string>? OnReadingNewFile;
        private static string? journalPath = string.Empty;
        public void StartWatching(JournalCommander cmdr)
        {
            settingsStore.SelectedCommanderID = cmdr.Id;
            _messagesReceived.Clear();
            journalPath = cmdr.JournalPath;

            if (string.IsNullOrEmpty(journalPath))
            {
                journalPath = JournalPath.GetJournalPath();
            }

            watcher = new(journalPath, true);
            watcher.MessageReceived += Watcher_MessageReceived;
            watcher.LiveStatusChange += OnLiveStatusChange;
            _ = watcher.StartWatchingFromFileOffset(cmdr.LastFile, 0).ConfigureAwait(false);
            OnReadingNewFile?.Invoke(this, journalPath);
        }

        private void OnLiveStatusChange(object? sender, bool e)
        {
            try
            {
                LiveStatusChange?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                loggerStore.LogError("Journal Watcher - Live Status", ex);
                _ = ODMessageBox.Show(null, "Error executing live status change event", $"See VPLog.txt in {App.BaseDirectory} for details");
            }
        } 

        private void Watcher_MessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            try
            {
                var tuple = Tuple.Create(e.Filename, e.Offset);

                if (_messagesReceived.ContainsKey(tuple))
                {
                    return;
                }

                switch (e.EventType)
                {
                    //If we start reading a new file, write event of previoud file to database
                    case JournalTypeEnum.Fileheader:
                        if (watcher != null && watcher.IsLive == false)
                            OnReadingNewFile?.Invoke(this, e.Filename);

                        //this isn't part 1 for a new log set, return
                        if (e.EventArgs is FileheaderEvent.FileheaderEventArgs fileheader)
                        {
                            legacy = fileheader.gameversion.Major < 4;

                            if (fileheader.part > 1)
                                return;
                        }

                      
                        if (this.journalCommander != null)
                        {
                            Parallel.ForEach(_messagesReceived, (message) =>
                            {
                                message.Value.CommanderID = journalCommander.Id;
                            });

                            voqooeDatabaseProvider.AddJournalEntries(_messagesReceived.Values);
                        }
                        _messagesReceived.Clear();
                        break;
                    case JournalTypeEnum.Commander:
                        if (e.EventArgs is CommanderEvent.CommanderEventArgs arg)
                        {
                            var journalCommander = new JournalCommander(
                                        -1,
                                        legacy ? $"{arg.Name} (Legacy)" : arg.Name,
                                        watcher?.Path,
                                        Path.GetFileName(watcher?.LatestJournalFile ?? string.Empty),
                                        false);

                            this.journalCommander = voqooeDatabaseProvider.AddCommander(journalCommander);
                        }
                        break;
                    default:
                        break;
                }

                var jEvent = new JournalEntry(
                    e.Filename,
                    e.Offset,
                    journalCommander?.Id ?? -1,
                    e.EventType,
                    e.EventArgs.Clone(),
                    e.JObject.DeepClone() as JObject);

                var added = _messagesReceived.TryAdd(tuple, jEvent);

                if (added && watcher != null)
                {
                    OnJournalEventRecieved?.Invoke(this, jEvent);
                }
            }
            catch (Exception ex)
            {
                loggerStore.LogError("Journal Watcher - Event parser", ex);
                _ = ODMessageBox.Show(null, $"Error Parsing Event - {e.EventType}", $"See VPLog.txt in {App.BaseDirectory} for details");
            }
        }

        public NavigationRoute? ReadNavRouteJson(int cmdrId)
        {
            if(watcher is null || journalCommander is null || string.IsNullOrEmpty(journalPath) || cmdrId != journalCommander.Id)
            {
                return null;
            }
            var path = Path.Combine(journalPath, "NavRoute.json");
            var json = File.ReadAllText(path);
            var route = NavigationRoute.FromJson(json);
            return route;
        }

        public void StopWatcher()
        {
            watcher?.StopWatching();
            watcher?.Dispose();
            journalCommander = null;
        }
    }
}
