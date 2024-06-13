namespace VoqooePlanner.Helpers
{
    public class OrganicInfo(string englishName, long value, int colonyRange)
    {
        public string EnglishName { get; } = englishName;
        public long Value { get; } = value;
        public int ColonyRange { get; } = colonyRange;
    }

    public static class OrganicValues
    {
        private static readonly Dictionary<string, OrganicInfo> bioValues = new(StringComparer.InvariantCultureIgnoreCase)
        {
            { "$Codex_Ent_Aleoids_01_Name;", new("Aleoida Arcus", 7252500, 150) },
            { "$Codex_Ent_Aleoids_02_Name;", new("Aleoida Coronamus", 6284600, 150) },
            { "$Codex_Ent_Aleoids_03_Name;", new("Aleoida Spica", 3385200, 150) },
            { "$Codex_Ent_Aleoids_04_Name;", new("Aleoida Laminiae", 3385200, 150) },
            { "$Codex_Ent_Aleoids_05_Name;", new("Aleoida Gravis", 12934900, 150) },

            { "$Codex_Ent_Bacterial_01_Name;", new("Bacterium Aurasus", 1000000, 500) },
            { "$Codex_Ent_Bacterial_02_Name;", new("Bacterium Nebulus", 5289900, 500) },
            { "$Codex_Ent_Bacterial_03_Name;", new("Bacterium Scopulum", 4934500, 500) },
            { "$Codex_Ent_Bacterial_04_Name;", new("Bacterium Acies", 1000000, 500) },
            { "$Codex_Ent_Bacterial_05_Name;", new("Bacterium Vesicula", 1000000, 500) },
            { "$Codex_Ent_Bacterial_06_Name;", new("Bacterium Alcyoneum", 1658500, 500) },
            { "$Codex_Ent_Bacterial_07_Name;", new("Bacterium Tela", 1949000, 500) },
            { "$Codex_Ent_Bacterial_08_Name;", new("Bacterium Informem", 8418000, 500) },
            { "$Codex_Ent_Bacterial_09_Name;", new("Bacterium Volu", 7774700, 500) },
            { "$Codex_Ent_Bacterial_10_Name;", new("Bacterium Bullaris", 1152500, 500) },
            { "$Codex_Ent_Bacterial_11_Name;", new("Bacterium Omentum", 4638900, 500) },
            { "$Codex_Ent_Bacterial_12_Name;", new("Bacterium Cerbrus", 1689800, 500) },
            { "$Codex_Ent_Bacterial_13_Name;", new("Bacterium Verrata", 3897000, 500) },


            { "$Codex_Ent_Cactoid_01_Name;", new("Cactoida Cortexum", 3667600, 300) },
            { "$Codex_Ent_Cactoid_02_Name;", new("Cactoida Lapis", 2483600, 300) },
            { "$Codex_Ent_Cactoid_03_Name;", new("Cactoida Vermis", 16202800, 300) },
            { "$Codex_Ent_Cactoid_05_Name;", new("Cactoida Peperatis", 2483600, 300) },

            { "$Codex_Ent_Clypeus_01_Name;", new("Clypeus Lacrimam", 8418000, 150) },
            { "$Codex_Ent_Clypeus_02_Name;", new("Clypeus Margaritus", 11873200, 150) },
            { "$Codex_Ent_Clypeus_03_Name;", new("Clypeus Speculumi", 16202800, 150) },

            { "$Codex_Ent_Conchas_01_Name;", new("Concha Renibus", 4572400, 150) },
            { "$Codex_Ent_Conchas_02_Name;", new("Concha Aureolas", 7774700, 150) },
            { "$Codex_Ent_Conchas_03_Name;", new("Concha Labiata", 2352400, 150) },
            { "$Codex_Ent_Conchas_04_Name;", new("Concha Biconcavis", 19010800, 150) },

            { "$Codex_Ent_Cone_Name;", new("Bark Mounds", 1471900, 100) },

            { "$Codex_Ent_Electricae_01_Name;", new("Electricae Pluma", 6284600, 1000) },
            { "$Codex_Ent_Electricae_02_Name;", new("Electricae Radialem", 6284600, 1000) },

            { "$Codex_Ent_Fonticulus_01_Name;", new("Fonticulua Segmentatus", 19010800, 500) },
            { "$Codex_Ent_Fonticulus_02_Name;", new("Fonticulua Campestris", 1000000, 500) },
            { "$Codex_Ent_Fonticulus_03_Name;", new("Fonticulua Upupam", 5727600, 500) },
            { "$Codex_Ent_Fonticulus_04_Name;", new("Fonticulua Lapida", 3111000, 500) },
            { "$Codex_Ent_Fonticulus_05_Name;", new("Fonticulua Fluctus", 20000000, 500) },
            { "$Codex_Ent_Fonticulus_06_Name;", new("Fonticulua Digitos", 1804100, 500) },

            { "$Codex_Ent_Fumerolas_01_Name;", new("Fumerola Carbosis", 6284600, 100) },
            { "$Codex_Ent_Fumerolas_02_Name;", new("Fumerola Extremus", 16202800, 100) },
            { "$Codex_Ent_Fumerolas_03_Name;", new("Fumerola Nitris", 7500900, 100) },
            { "$Codex_Ent_Fumerolas_04_Name;", new("Fumerola Aquatis", 6284600, 100) },

            { "$Codex_Ent_Fungoids_01_Name;", new("Fungoida Setisis", 1670100, 300) },
            { "$Codex_Ent_Fungoids_02_Name;", new("Fungoida Stabitis", 2680300, 300) },
            { "$Codex_Ent_Fungoids_03_Name;", new("Fungoida Bullarum", 3703200, 300) },
            { "$Codex_Ent_Fungoids_04_Name;", new("Fungoida Gelata", 3330300, 300) },

            { "$Codex_Ent_Osseus_01_Name;", new("Osseus Fractus", 4027800, 800) },
            { "$Codex_Ent_Osseus_02_Name;", new("Osseus Discus", 12934900, 800) },
            { "$Codex_Ent_Osseus_03_Name;", new("Osseus Spiralis", 2404700, 800) },
            { "$Codex_Ent_Osseus_04_Name;", new("Osseus Pumice", 3156300, 800) },
            { "$Codex_Ent_Osseus_05_Name;", new("Osseus Cornibus", 1483000, 800) },
            { "$Codex_Ent_Osseus_06_Name;", new("Osseus Pellebantus", 9739000, 800) },

            { "$Codex_Ent_Recepta_01_Name;", new("Recepta Umbrux", 12934900, 150) },
            { "$Codex_Ent_Recepta_02_Name;", new("Recepta Deltahedronix", 16202800, 150) },
            { "$Codex_Ent_Recepta_03_Name;", new("Recepta Conditivus", 14313700, 150) },

            { "$Codex_Ent_Seed_Name;", new("Roseum Brain Tree", 1593700, 100) },

            { "$Codex_Ent_Shrubs_01_Name;", new("Frutexa Flabellum", 1808900, 150) },
            { "$Codex_Ent_Shrubs_02_Name;", new("Frutexa Acus", 7774700, 150) },
            { "$Codex_Ent_Shrubs_03_Name;", new("Frutexa Metallicum", 1632500, 150) },
            { "$Codex_Ent_Shrubs_04_Name;", new("Frutexa Flammasis", 10326000, 150) },
            { "$Codex_Ent_Shrubs_05_Name;", new("Frutexa Fera", 1632500, 150) },
            { "$Codex_Ent_Shrubs_06_Name;", new("Frutexa Sponsae", 5988000, 150) },
            { "$Codex_Ent_Shrubs_07_Name;", new("Frutexa Collum", 1639800, 150) },

            { "$Codex_Ent_SphereEFGH_01_Name;", new("Rubeum Bioluminescent Anemone", 1499900, 100) },
            { "$Codex_Ent_SphereEFGH_02_Name;", new("Prasinum Bioluminescent Anemone", 1499900, 100) },
            { "$Codex_Ent_SphereEFGH_03_Name;", new("Roseum Bioluminescent Anemone", 1499900, 100) },
            { "$Codex_Ent_SphereEFGH_Name;", new("Blatteum Bioluminescent Anemone", 1499900, 100) },

            { "$Codex_Ent_Stratum_01_Name;", new("Stratum Excutitus", 2448900, 500) },
            { "$Codex_Ent_Stratum_02_Name;", new("Stratum Paleas", 1362000, 500) },
            { "$Codex_Ent_Stratum_03_Name;", new("Stratum Laminamus", 2788300, 500) },
            { "$Codex_Ent_Stratum_04_Name;", new("Stratum Araneamus", 2448900, 500) },
            { "$Codex_Ent_Stratum_05_Name;", new("Stratum Limaxus", 1362000, 500) },
            { "$Codex_Ent_Stratum_06_Name;", new("Stratum Cucumisis", 16202800, 500) },
            { "$Codex_Ent_Stratum_07_Name;", new("Stratum Tectonicas", 19010800, 500) },

            { "$Codex_Ent_TubeABCD_03_Name;", new("Caeruleum Sinuous Tubers", 1514500, 100) },
            { "$Codex_Ent_TubeEFGH_Name;", new("Blatteum Sinuous Tubers", 3425600, 200) },

            { "$Codex_Ent_Tubus_02_Name;", new("Tubus Sororibus", 5727600, 800) },
            { "$Codex_Ent_Tubus_03_Name;", new("Tubus Cavas", 11873200, 800) },
            { "$Codex_Ent_Tubus_04_Name;", new("Tubus Rosarium", 2637500, 800) },
            { "$Codex_Ent_Tubus_05_Name;", new("Tubus Compagibus", 7774700, 800) },

            { "$Codex_Ent_Tussocks_01_Name;", new("Tussock Pennata", 5853800, 200) },
            { "$Codex_Ent_Tussocks_02_Name;", new("Tussock Ventusa", 3227700, 200) },
            { "$Codex_Ent_Tussocks_03_Name;", new("Tussock Ignis", 1849000, 200) },
            { "$Codex_Ent_Tussocks_04_Name;", new("Tussock Cultro", 1766600, 200) },
            { "$Codex_Ent_Tussocks_05_Name;", new("Tussock Catena", 1766600, 200) },
            { "$Codex_Ent_Tussocks_06_Name;", new("Tussock Pennatis", 1000000, 200) },
            { "$Codex_Ent_Tussocks_07_Name;", new("Tussock Serrati", 4447100, 200) },
            { "$Codex_Ent_Tussocks_08_Name;", new("Tussock Albata", 3252500, 200) },
            { "$Codex_Ent_Tussocks_09_Name;", new("Tussock Propagito", 1000000, 200) },
            { "$Codex_Ent_Tussocks_10_Name;", new("Tussock Divisa", 1766600, 200) },
            { "$Codex_Ent_Tussocks_11_Name;", new("Tussock Caputus", 3472400, 200) },
            { "$Codex_Ent_Tussocks_12_Name;", new("Tussock Triticum", 7774700, 200) },
            { "$Codex_Ent_Tussocks_13_Name;", new("Tussock Stigmasis", 19010800, 200) },
            { "$Codex_Ent_Tussocks_14_Name;", new("Tussock Virgam", 14313700, 200) },
            { "$Codex_Ent_Tussocks_15_Name;", new("Tussock Capillum", 7025800, 200) },

            { "$Codex_Ent_Vents_Name;", new("Amphora Plant", 3626400, 100) },
        };

        public static OrganicInfo GetOrganicInfo(string speciesCodexValue, string localName)
        {
            if(string.IsNullOrEmpty(speciesCodexValue))
            {
                return new(localName, 0, 100);
            }

            if(bioValues.TryGetValue(speciesCodexValue, out var bioValue))
            {
                return bioValue;
            }

            return new(localName, 0, 100);
        }
    }
}
