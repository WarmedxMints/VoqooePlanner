using ODUtils.Models;

namespace VoqooePlanner.ViewModels.ModelViews
{
    public class RouteStopViewModel : ViewModelBase
    {
        public RouteStopViewModel(VoqooeSystemViewModel starSystem, VoqooeSystemViewModel? prevSystem = null)
        {
            this.starSystem = starSystem;
            this.prevSystem = prevSystem;
        }

        public RouteStopViewModel(VoqooeSystemViewModel startSystem, JournalSystemViewModel journalSystem)
        {
            this.starSystem = startSystem;
            this.prevSystem = new VoqooeSystemViewModel(new(journalSystem));
        }
        private readonly VoqooeSystemViewModel starSystem;
        private readonly VoqooeSystemViewModel? prevSystem;

        public string Name => starSystem.Name ?? "Unknown";
        public string PrevSystemName => prevSystem?.Name ?? "Unknown";
        public string JumpDistance => prevSystem == null ? string.Empty : $"{Distance:N2} ly";
        public double Distance => prevSystem == null ? 0 : starSystem.Pos.DistanceFrom(prevSystem.Pos);
        public Position Pos => starSystem.Pos * 2;

    }
}
