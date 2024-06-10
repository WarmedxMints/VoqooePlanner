using VoqooePlanner.Models;

namespace VoqooePlanner.ViewModels.ModelViews
{
    public sealed class OrganicScanDetailsViewModel : ViewModelBase
    {
        public string CodexValue { get; set; } = string.Empty;

        private string name = string.Empty;
        public string Name
        {
            get
            {
                if (UseLocalName && string.IsNullOrEmpty(LocalName) == false)
                {
                    return LocalName;
                }
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        private string? localName = string.Empty;
        public string? LocalName { get => localName; set { localName = value; OnPropertyChanged(nameof(LocalName)); } }

        private bool usLocalName = true;
        public bool UseLocalName
        {
            get => usLocalName;
            set
            {
                usLocalName = value;
                OnPropertyChanged(nameof(UseLocalName));
                OnPropertyChanged(nameof(Name));
            }
        }

        private OrganicScanState state;
        public OrganicScanState State
        {
            get => state ;
            set
            {
                state = value;
                OnPropertyChanged(nameof(State));
            }
        }

        public void UpdateFromEvent(OrganicScanDetails details)
        {
            LocalName = details.Species_Localised;
            Name = details.Species;
            State = details.State;
        }

        public void SetLocalNameUse(bool useLocalName)
        {
            UseLocalName = useLocalName;
        }
    }
}
