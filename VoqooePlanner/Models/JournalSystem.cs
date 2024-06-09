using EliteJournalReader;

namespace VoqooePlanner.Models
{
    public sealed class JournalSystem(long address, SystemPosition pos, string name)
    {
        public long Address { get; } = address;
        public string Name { get; } = name;
        public SystemPosition Pos { get; } = pos;
    }
}