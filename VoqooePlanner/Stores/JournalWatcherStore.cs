﻿using EliteJournalReader.Events;
using EliteJournalReader;
using VoqooePlanner.Models;
using VoqooePlanner.Services.Database;
using System.Collections.Concurrent;
using ODUtils.EliteDangerousHelpers;
using Newtonsoft.Json.Linq;
using System.IO;

namespace VoqooePlanner.Stores
{
    public sealed class JournalWatcherStore(SettingsStore settingsStore, IVoqooeDatabaseProvider voqooeDatabaseProvider)
    {
        private readonly SettingsStore settingsStore = settingsStore;
        private readonly IVoqooeDatabaseProvider voqooeDatabaseProvider = voqooeDatabaseProvider;

        private JournalWatcher? watcher;
        private JournalCommander? journalCommander;
        private JournalCommander? currentCommader;
        private readonly ConcurrentDictionary<Tuple<string, long>, JournalEntry> _messagesReceived = [];

        public EventHandler<VoqooeSystem>? OnCurrentSystemChanged;

        public EventHandler<bool>? LiveStatusChange;
        public EventHandler<JournalEntry>? OnJournalEventRecieved;
        public EventHandler<string>? OnReadingNewFile;

        public void StartWatching(JournalCommander cmdr)
        {
            settingsStore.SelectedCommanderID = cmdr.Id;
            currentCommader = cmdr;
            _messagesReceived.Clear();
            var path = cmdr.JournalPath;

            if(string.IsNullOrEmpty(path))
            {
                path = JournalPath.GetJournalPath();
            }

            watcher = new(path);
            watcher.MessageReceived += Watcher_MessageReceived;
            watcher.LiveStatusChange += OnLiveStatusChange;
            _ = watcher.StartWatchingFromFileOffset(cmdr.LastFile, 0);
        }

        private void OnLiveStatusChange(object? sender, bool e)
        {
            LiveStatusChange?.Invoke(this, e);
        }

        private void Watcher_MessageReceived(object? sender, MessageReceivedEventArgs e)
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
                    if(watcher != null && watcher.IsLive == false)
                        OnReadingNewFile?.Invoke(this, e.Filename);

                    if (this.journalCommander != null)
                    {
                        Parallel.ForEach(_messagesReceived, (message) =>
                        {
                            message.Value.CommanderID = journalCommander.Id;
                        });
                       
                        voqooeDatabaseProvider.AddJournalEntries(_messagesReceived.Values);
                        _messagesReceived.Clear();
                    }
                    break;
                case JournalTypeEnum.Commander:
                    if (e.EventArgs is CommanderEvent.CommanderEventArgs arg)
                    {
                        var journalCommander = new JournalCommander(
                                    -1,
                                    arg.Name,
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

            if (added)
            {
                OnJournalEventRecieved?.Invoke(this, jEvent);
            }
        }

        public void StopWatcher()
        {
            watcher?.StopWatching();
            watcher?.Dispose();
            journalCommander = null;
        }
    }
}
