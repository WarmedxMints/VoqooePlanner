using VoqooePlanner.Models;

namespace VoqooePlanner.ViewModels.ModelViews
{
    public sealed class StarSystemViewModel
    {
        private readonly StarSystem system;

        public StarSystemViewModel(StarSystem system)
        {
            this.system = system;
            Bodies = system.SystemBodies
                .OrderBy(x => x.BodyID)
                .Select(x => new SystemBodyViewModel(x))
                .ToList();
        }
        public long Address => system.Address;
        public string Name => system.Name;
        public int ScannedBodiesValue => Bodies.Where(x => x.IsNonBody == false).Count();
        public string ScannedBodies => ScannedBodiesValue.ToString("N0");
        public int DiscoveredBodesValue => system.BodyCount;
        public string DiscoveredBodes => DiscoveredBodesValue.ToString("N0");
        public long Value => Bodies.Sum(x => x.UserValue);
        public string StringValue => Value.ToString("N0");
        public List<SystemBodyViewModel> Bodies { get; } = [];
    }
}
