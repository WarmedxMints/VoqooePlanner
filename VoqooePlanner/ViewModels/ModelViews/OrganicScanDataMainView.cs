using System.Windows;
using VoqooePlanner.Models;

namespace VoqooePlanner.ViewModels.ModelViews
{
    public sealed class OrganicScanDataMainView : ViewModelBase
    {
        private Dictionary<string, List<OrganicScanDetailsViewModel>> OrganicScanDetails = new()
        {
            { "$Codex_Ent_Aleoids_Genus_Name;", [ CreateView("$Codex_Ent_Aleoids_01_Name;", "Aleoida Arcus"), CreateView("$Codex_Ent_Aleoids_02_Name;", "Aleoida Coronamus"), CreateView("$Codex_Ent_Aleoids_05_Name;", "Aleoida Gravis"), CreateView("$Codex_Ent_Aleoids_04_Name;", "Aleoida Laminiae"), CreateView("$Codex_Ent_Aleoids_03_Name;", "Aleoida Spica"),] },
            { "$Codex_Ent_Bacterial_Genus_Name;", [ CreateView("$Codex_Ent_Bacterial_04_Name;", "Bacterium Acies"), CreateView("$Codex_Ent_Bacterial_06_Name;", "Bacterium Alcyoneum"), CreateView("$Codex_Ent_Bacterial_01_Name;", "Bacterium Aurasus"), CreateView("$Codex_Ent_Bacterial_10_Name;", "Bacterium Bullaris"), CreateView("$Codex_Ent_Bacterial_12_Name;", "Bacterium Cerbrus"), CreateView("$Codex_Ent_Bacterial_08_Name;", "Bacterium Informem"), CreateView("$Codex_Ent_Bacterial_02_Name;", "Bacterium Nebulus"), CreateView("$Codex_Ent_Bacterial_11_Name;", "Bacterium Omentum"), CreateView("$Codex_Ent_Bacterial_03_Name;", "Bacterium Scopulum"), CreateView("$Codex_Ent_Bacterial_07_Name;", "Bacterium Tela"), CreateView("$Codex_Ent_Bacterial_13_Name;", "Bacterium Verrata"), CreateView("$Codex_Ent_Bacterial_05_Name;", "Bacterium Vesicula"), CreateView("$Codex_Ent_Bacterial_09_Name;", "Bacterium Volu"),] },
            { "$Codex_Ent_Cactoid_Genus_Name;", [ CreateView("$Codex_Ent_Cactoid_01_Name;", "Cactoida Cortexum"), CreateView("$Codex_Ent_Cactoid_02_Name;", "Cactoida Lapis"), CreateView("$Codex_Ent_Cactoid_05_Name;", "Cactoida Peperatis"), CreateView("$Codex_Ent_Cactoid_04_Name;", "Cactoida Pullulanta"), CreateView("$Codex_Ent_Cactoid_03_Name;", "Cactoida Vermis"),] },
            { "$Codex_Ent_Clepeus_Genus_Name;", [ CreateView("$Codex_Ent_Clypeus_01_Name;", "Clypeus Lacrimam"), CreateView("$Codex_Ent_Clypeus_02_Name;", "Clypeus Margaritus"),] },
            { "$Codex_Ent_Clypeus_Genus_Name;", [ CreateView("$Codex_Ent_Clypeus_01_Name;", "Clypeus Lacrimam"), CreateView("$Codex_Ent_Clypeus_02_Name;", "Clypeus Margaritus"), CreateView("$Codex_Ent_Clypeus_03_Name;", "Clypeus Speculumi"),] },
            { "$Codex_Ent_Conchas_Genus_Name;", [ CreateView("$Codex_Ent_Conchas_02_Name;", "Concha Aureolas"), CreateView("$Codex_Ent_Conchas_04_Name;", "Concha Biconcavis"), CreateView("$Codex_Ent_Conchas_03_Name;", "Concha Labiata"), CreateView("$Codex_Ent_Conchas_01_Name;", "Concha Renibus"),] },
            { "$Codex_Ent_Electricae_Genus_Name;", [ CreateView("$Codex_Ent_Electricae_01_Name;", "Electricae Pluma"), CreateView("$Codex_Ent_Electricae_02_Name;", "Electricae Radialem"),] },
            { "$Codex_Ent_Fonticulus_Genus_Name;", [ CreateView("$Codex_Ent_Fonticulus_02_Name;", "Fonticulua Campestris"), CreateView("$Codex_Ent_Fonticulus_06_Name;", "Fonticulua Digitos"), CreateView("$Codex_Ent_Fonticulus_05_Name;", "Fonticulua Fluctus"), CreateView("$Codex_Ent_Fonticulus_04_Name;", "Fonticulua Lapida"), CreateView("$Codex_Ent_Fonticulus_01_Name;", "Fonticulua Segmentatus"), CreateView("$Codex_Ent_Fonticulus_03_Name;", "Fonticulua Upupam"),] },
            { "$Codex_Ent_Fumerolas_Genus_Name;", [ CreateView("$Codex_Ent_Fumerolas_04_Name;", "Fumerola Aquatis"), CreateView("$Codex_Ent_Fumerolas_01_Name;", "Fumerola Carbosis"), CreateView("$Codex_Ent_Fumerolas_02_Name;", "Fumerola Extremus"), CreateView("$Codex_Ent_Fumerolas_03_Name;", "Fumerola Nitris"),] },
            { "$Codex_Ent_Fungoids_Genus_Name;", [ CreateView("$Codex_Ent_Fungoids_03_Name;", "Fungoida Bullarum"), CreateView("$Codex_Ent_Fungoids_04_Name;", "Fungoida Gelata"), CreateView("$Codex_Ent_Fungoids_01_Name;", "Fungoida Setisis"), CreateView("$Codex_Ent_Fungoids_02_Name;", "Fungoida Stabitis"),] },
            { "$Codex_Ent_Osseus_Genus_Name;", [ CreateView("$Codex_Ent_Osseus_05_Name;", "Osseus Cornibus"), CreateView("$Codex_Ent_Osseus_02_Name;", "Osseus Discus"), CreateView("$Codex_Ent_Osseus_01_Name;", "Osseus Fractus"), CreateView("$Codex_Ent_Osseus_06_Name;", "Osseus Pellebantus"), CreateView("$Codex_Ent_Osseus_04_Name;", "Osseus Pumice"), CreateView("$Codex_Ent_Osseus_03_Name;", "Osseus Spiralis"),] },
            { "$Codex_Ent_Recepta_Genus_Name;", [ CreateView("$Codex_Ent_Recepta_03_Name;", "Recepta Conditivus"), CreateView("$Codex_Ent_Recepta_02_Name;", "Recepta Deltahedronix"), CreateView("$Codex_Ent_Recepta_01_Name;", "Recepta Umbrux"),] },
            { "$Codex_Ent_Shrubs_Genus_Name;", [ CreateView("$Codex_Ent_Shrubs_02_Name;", "Frutexa Acus"), CreateView("$Codex_Ent_Shrubs_07_Name;", "Frutexa Collum"), CreateView("$Codex_Ent_Shrubs_05_Name;", "Frutexa Fera"), CreateView("$Codex_Ent_Shrubs_01_Name;", "Frutexa Flabellum"), CreateView("$Codex_Ent_Shrubs_04_Name;", "Frutexa Flammasis"), CreateView("$Codex_Ent_Shrubs_03_Name;", "Frutexa Metallicum"), CreateView("$Codex_Ent_Shrubs_06_Name;", "Frutexa Sponsae"),] },
            { "$Codex_Ent_Stratum_Genus_Name;", [ CreateView("$Codex_Ent_Stratum_04_Name;", "Stratum Araneamus"), CreateView("$Codex_Ent_Stratum_06_Name;", "Stratum Cucumisis"), CreateView("$Codex_Ent_Stratum_01_Name;", "Stratum Excutitus"), CreateView("$Codex_Ent_Stratum_08_Name;", "Stratum Frigus"), CreateView("$Codex_Ent_Stratum_03_Name;", "Stratum Laminamus"), CreateView("$Codex_Ent_Stratum_05_Name;", "Stratum Limaxus"), CreateView("$Codex_Ent_Stratum_02_Name;", "Stratum Paleas"), CreateView("$Codex_Ent_Stratum_07_Name;", "Stratum Tectonicas"),] },
            { "$Codex_Ent_Tubus_Genus_Name;", [ CreateView("$Codex_Ent_Tubus_03_Name;", "Tubus Cavas"), CreateView("$Codex_Ent_Tubus_05_Name;", "Tubus Compagibus"), CreateView("$Codex_Ent_Tubus_01_Name;", "Tubus Conifer"), CreateView("$Codex_Ent_Tubus_04_Name;", "Tubus Rosarium"), CreateView("$Codex_Ent_Tubus_02_Name;", "Tubus Sororibus"),] },
            { "$Codex_Ent_Tussocks_Genus_Name;", [ CreateView("$Codex_Ent_Tussocks_08_Name;", "Tussock Albata"), CreateView("$Codex_Ent_Tussocks_15_Name;", "Tussock Capillum"), CreateView("$Codex_Ent_Tussocks_11_Name;", "Tussock Caputus"), CreateView("$Codex_Ent_Tussocks_05_Name;", "Tussock Catena"), CreateView("$Codex_Ent_Tussocks_04_Name;", "Tussock Cultro"), CreateView("$Codex_Ent_Tussocks_10_Name;", "Tussock Divisa"), CreateView("$Codex_Ent_Tussocks_03_Name;", "Tussock Ignis"), CreateView("$Codex_Ent_Tussocks_01_Name;", "Tussock Pennata"), CreateView("$Codex_Ent_Tussocks_06_Name;", "Tussock Pennatis"), CreateView("$Codex_Ent_Tussocks_09_Name;", "Tussock Propagito"), CreateView("$Codex_Ent_Tussocks_07_Name;", "Tussock Serrati"), CreateView("$Codex_Ent_Tussocks_13_Name;", "Tussock Stigmasis"), CreateView("$Codex_Ent_Tussocks_12_Name;", "Tussock Triticum"), CreateView("$Codex_Ent_Tussocks_02_Name;", "Tussock Ventusa"), CreateView("$Codex_Ent_Tussocks_14_Name;", "Tussock Virgam"),] },
            { "Other", [] }
        };
        public IEnumerable<OrganicScanDetailsViewModel> Aleoida => OrganicScanDetails["$Codex_Ent_Aleoids_Genus_Name;"];
        public IEnumerable<OrganicScanDetailsViewModel> Bacterium => OrganicScanDetails["$Codex_Ent_Bacterial_Genus_Name;"];
        public IEnumerable<OrganicScanDetailsViewModel> Cactoida => OrganicScanDetails["$Codex_Ent_Cactoid_Genus_Name;"];
        public IEnumerable<OrganicScanDetailsViewModel> Clypeus => OrganicScanDetails["$Codex_Ent_Clypeus_Genus_Name;"];
        public IEnumerable<OrganicScanDetailsViewModel> Concha => OrganicScanDetails["$Codex_Ent_Conchas_Genus_Name;"];
        public IEnumerable<OrganicScanDetailsViewModel> Electricae => OrganicScanDetails["$Codex_Ent_Electricae_Genus_Name;"];
        public IEnumerable<OrganicScanDetailsViewModel> Fonticulua => OrganicScanDetails["$Codex_Ent_Fonticulus_Genus_Name;"];
        public IEnumerable<OrganicScanDetailsViewModel> Fumerola => OrganicScanDetails["$Codex_Ent_Fumerolas_Genus_Name;"];
        public IEnumerable<OrganicScanDetailsViewModel> Fungoida => OrganicScanDetails["$Codex_Ent_Fungoids_Genus_Name;"];
        public IEnumerable<OrganicScanDetailsViewModel> Osseus => OrganicScanDetails["$Codex_Ent_Osseus_Genus_Name;"];
        public IEnumerable<OrganicScanDetailsViewModel> Recepta => OrganicScanDetails["$Codex_Ent_Recepta_Genus_Name;"];
        public IEnumerable<OrganicScanDetailsViewModel> Frutexa => OrganicScanDetails["$Codex_Ent_Shrubs_Genus_Name;"];
        public IEnumerable<OrganicScanDetailsViewModel> Stratum => OrganicScanDetails["$Codex_Ent_Stratum_Genus_Name;"];
        public IEnumerable<OrganicScanDetailsViewModel> Tubus => OrganicScanDetails["$Codex_Ent_Tubus_Genus_Name;"];
        public IEnumerable<OrganicScanDetailsViewModel> Tussock => OrganicScanDetails["$Codex_Ent_Tussocks_Genus_Name;"];
        public IEnumerable<OrganicScanDetailsViewModel> Other => OrganicScanDetails["Other"];

        private string totalCount = string.Empty;
        public string TotalCount
        {
            get => totalCount;
            set
            {
                totalCount = value;
                OnPropertyChanged(nameof(TotalCount));
            }
        }

        private Visibility otherVisiblity;
        public Visibility OtherVisiblity
        {
            get => otherVisiblity;
            set
            {
                otherVisiblity = value;
                OnPropertyChanged(nameof(OtherVisiblity));
            }
        }
        private static OrganicScanDetailsViewModel CreateView(string codexValue, string name)
        {
            return new OrganicScanDetailsViewModel()
            {
                CodexValue = codexValue,
                Name = name,
                State = OrganicScanState.None
            };
        }

        public void AddScanDetatil(IEnumerable<OrganicScanDetails> organicScanDetails)
        {
            var genusToupate = new List<string>();

            foreach (var organic in organicScanDetails)
            {
                if (genusToupate.Contains(organic.Genus) == false)
                {
                    genusToupate.Add(organic.Genus);
                }

                if (OrganicScanDetails.TryGetValue(organic.Genus, out var value))
                {
                    UpdateOrganic(organic, value);
                    continue;
                }

                var listTOEdit = OrganicScanDetails["Other"];

                UpdateOrganic(organic, listTOEdit);
            }

            foreach(var genus in genusToupate)
            {
                FireUpdateProertyChanged(genus);
            }

            var soldCount = 0;
            var totalCount = 0;
            foreach (var item in OrganicScanDetails.Values)
            {
                totalCount += item.Count;
                soldCount += item.Count(x => x.State == OrganicScanState.Sold);
            }
            TotalCount = $"{soldCount:N0}/{totalCount:N0} Sold";

            OtherVisiblity = Other.Count() > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void FireUpdateProertyChanged(string key)
        {
            switch (key)
            {
                case "$Codex_Ent_Aleoids_Genus_Name;":
                    OnPropertyChanged(nameof(Aleoida));
                    break;
                case "$Codex_Ent_Bacterial_Genus_Name;":
                    OnPropertyChanged(nameof(Bacterium));
                    break;
                case "$Codex_Ent_Cactoid_Genus_Name;":
                    OnPropertyChanged(nameof(Cactoida));
                    break;
                case "$Codex_Ent_Clypeus_Genus_Name;":
                    OnPropertyChanged(nameof(Clypeus));
                    break;
                case "$Codex_Ent_Conchas_Genus_Name;":
                    OnPropertyChanged(nameof(Concha));
                    break;
                case "$Codex_Ent_Electricae_Genus_Name":
                    OnPropertyChanged(nameof(Electricae));
                    break;
                case "$Codex_Ent_Fonticulus_Genus_Name":
                    OnPropertyChanged(nameof(Fonticulua));
                    break;
                case "$Codex_Ent_Fumerolas_Genus_Name;":
                    OnPropertyChanged(nameof(Fumerola));
                    break;
                case "$Codex_Ent_Fungoids_Genus_Name;":
                    OnPropertyChanged(nameof(Fungoida));
                    break;
                case "$Codex_Ent_Osseus_Genus_Name;":
                    OnPropertyChanged(nameof(Osseus));
                    break;
                case "$Codex_Ent_Recepta_Genus_Name;":
                    OnPropertyChanged(nameof(Recepta));
                    break;
                case "$Codex_Ent_Shrubs_Genus_Name;":
                    OnPropertyChanged(nameof(Frutexa));
                    break;
                case "$Codex_Ent_Stratum_Genus_Name;":
                    OnPropertyChanged(nameof(Stratum));
                    break;
                case "$Codex_Ent_Tubus_Genus_Name;":
                    OnPropertyChanged(nameof(Tubus));
                    break;
                case "$Codex_Ent_Tussocks_Genus_Name;":
                    OnPropertyChanged(nameof(Tussock));
                    break;
                case "Other":
                    OnPropertyChanged(nameof(Other));
                    break;
            }
        }

        private static void UpdateOrganic(OrganicScanDetails organic, List<OrganicScanDetailsViewModel> value)
        {
            var known = value.FirstOrDefault(x => x.CodexValue.Equals(organic.Species, StringComparison.OrdinalIgnoreCase));

            if (known != null)
            {
                known.UpdateFromEvent(organic);
                return;
            }

            var organicToAdd = new OrganicScanDetailsViewModel()
            {
                CodexValue = organic.Species,
                LocalName = organic.Species_Localised,
                State = organic.State,
            };

            value.Add(organicToAdd);
        }
    }
}
