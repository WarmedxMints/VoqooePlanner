namespace VoqooePlanner.Models
{
    public sealed class IgnoredSystem
    {
        public IgnoredSystem(long address, string name, int cmdrId)
        {
            Address = address;
            Name = name;
            CmdrId = cmdrId;
        }

        public long Address { get; }
        public string Name { get; }
        public int CmdrId { get; }
    }
}
