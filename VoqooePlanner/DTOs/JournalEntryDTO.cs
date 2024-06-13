namespace VoqooePlanner.DTOs
{
    public sealed class JournalEntryDTO
    {
        public required string Filename { get; set; }
        public required long Offset { get; set; } 
        public DateTime TimeStamp { get; set; }
        public required int CommanderID { get; set; }
        public required int EventTypeId { get; set; }
        public required string EventData { get; set; }
    }
}
