using ODUtils.Models;

namespace VoqooePlanner.ViewModels.ModelViews
{
    public class RouteStopViewModel(VoqooeSystemViewModel starSystem, VoqooeSystemViewModel? prevSystem = null) : ViewModelBase
    {
        private readonly VoqooeSystemViewModel starSystem = starSystem;
        private readonly VoqooeSystemViewModel? prevSystem = prevSystem;

        public string Name => starSystem.Name ?? "Unknown";
        public string PrevSystemName => prevSystem?.Name ?? "Unknown";
        public string JumpDistance => prevSystem == null ? string.Empty : $"{Distance:N2} ly";
        public double Distance => prevSystem == null ? 0 : starSystem.Pos.DistanceFrom(prevSystem.Pos);
        public Position Pos => starSystem.Pos * 2;
    }
}
