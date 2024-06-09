namespace VoqooePlanner.Models
{
    public struct UpdateInfo
    {
        public Version Version { get; set; }
        public string[] PatchNotes { get; set; }
        public string Url { get; set; }
    }
}
