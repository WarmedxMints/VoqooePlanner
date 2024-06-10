namespace VoqooePlanner.Models
{
    public sealed class OrganicScanDetails(string species, string species_localised, string genus, string genus_localised)
    {
        public string Species { get; } = species;
        public string Species_Localised { get; } = species_localised;
        public string Genus { get; } = genus;
        public string Genus_Localised { get; } = genus_localised;
        public OrganicScanState State { get; set; }
    }
}
