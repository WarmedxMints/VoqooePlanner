using EliteJournalReader;
using ODUtils.Models;
using System.Windows;
using System.Windows.Media;
using VoqooePlanner.Models;

namespace VoqooePlanner.ViewModels
{
    public class VoqooeSystemViewModel(VoqooeSystem system) : ViewModelBase
    {
        private readonly VoqooeSystem system = system;

        public VoqooeSystem System => system;
        public Brush Scoopable
        {
            get
            {
                var starType = (StarType)system.StarType;

                return starType switch
                {
                    EliteJournalReader.StarType.K or EliteJournalReader.StarType.G or EliteJournalReader.StarType.B or EliteJournalReader.StarType.F or EliteJournalReader.StarType.O or EliteJournalReader.StarType.A or EliteJournalReader.StarType.M => (SolidColorBrush)Application.Current.Resources["ScoopableStar"],
                    EliteJournalReader.StarType.N => (SolidColorBrush)Application.Current.Resources["NeutronStar"],
                    _ => (SolidColorBrush)Application.Current.Resources["NonScoopableStar"],
                };
            }
        }
        public string StarType => system.StarType == 0 ? "?" : ((StarType)system.StarType).ToString();
        public long Address => system.Address;
        public string Name => system.Name;
        public string ContainsELW => system.ContainsELW ? "\xE8FB" : string.Empty;
        public string Visited => system.Visited ? "\xE8FB" : string.Empty;
        public string UserVisited => system.UserVisited ? "\xE8FB" : string.Empty;
        public string Value => system.Value > 0 ? system.Value.ToString("N0") : string.Empty;
        public string Distance => system.Distance.ToString("N2");

        public Position Pos => new(system.X, system.Y, system.Z);
    }
}