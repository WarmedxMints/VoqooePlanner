using EliteJournalReader;
using Newtonsoft.Json.Linq;

namespace VoqooePlanner.Models
{
    public sealed class JournalEntry(string filename, long offset, int commanderID, JournalTypeEnum eventTypeId, JournalEventArgs eventData, JObject? originalEvent)
    {
        public string Filename { get; } = filename;
        public long Offset { get; } = offset;
        public int CommanderID { get; set; } = commanderID;
        public JournalTypeEnum EventType { get; } = eventTypeId;
        public JournalEventArgs EventData { get; } = eventData;
        public JObject? OriginalEvent { get; } = originalEvent;
        public DateTime TimeStamp { get; } = eventData.Timestamp;
    }
}
