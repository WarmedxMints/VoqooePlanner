namespace VoqooePlanner.EventArgs
{
    public sealed class CartoDataUpdateArgs(long systemAddress, long bodyId)
    {
        public long SystemAddress { get; } = systemAddress;
        public long BodyId { get; } = bodyId;
    }
}
