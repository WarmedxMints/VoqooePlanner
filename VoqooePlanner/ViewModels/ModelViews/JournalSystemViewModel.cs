using EliteJournalReader;
using VoqooePlanner.Models;

namespace VoqooePlanner.ViewModels.ModelViews
{
    public sealed class JournalSystemViewModel(JournalSystem system, double distance) : ViewModelBase
    {
        private readonly JournalSystem _journalSystem = system;
        private readonly double _distanceFromHub = distance;

        public SystemPosition Pos => _journalSystem.Pos;
        public string Name => _journalSystem.Name;
        public string X => _journalSystem.Pos.X.ToString("N2");
        public string Y => _journalSystem.Pos.Y.ToString("N2");
        public string Z => _journalSystem.Pos.Z.ToString("N2");
        public string DistanceFromHub => $"{_distanceFromHub:N2} ly from Hub";
    }
}
