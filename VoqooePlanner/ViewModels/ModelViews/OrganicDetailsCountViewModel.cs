namespace VoqooePlanner.ViewModels.ModelViews
{
    public sealed class OrganicDetailsCountViewModel(string name, long count, long value)
    {
        public string Name { get; } = name;
        public long Count { get; } = count;
        private long value { get; } = value;
        public string Value => value == 0 ? string.Empty : $"{value:N0}";
    }
}
