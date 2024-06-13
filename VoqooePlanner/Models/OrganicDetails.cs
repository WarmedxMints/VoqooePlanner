namespace VoqooePlanner.Models
{
    public sealed class OrganicDetails(string name, string bodyName, string species_local, string speciesCodex)
    {
        public string Name { get; } = name;
        public string BodyName { get; } = bodyName;
        public string Species_Local { get; } = species_local;
        public string SpeciesCodex { get; } = speciesCodex;
    }
}
