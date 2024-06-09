namespace VoqooePlanner.Models
{
    public sealed class JournalCommander(int id, string name, string? journalPath, string? lastFile, bool hidden)
    {
        public int Id { get; } = id;
        public string Name { get; } = name;
        public string? JournalPath { get; set; } = journalPath;
        public string? LastFile { get; set; } = lastFile;
        public bool IsHidden { get; set; } = hidden;
    }
}