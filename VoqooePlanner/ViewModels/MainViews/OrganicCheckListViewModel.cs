using VoqooePlanner.Models;
using VoqooePlanner.Stores;
using VoqooePlanner.ViewModels.ModelViews;

namespace VoqooePlanner.ViewModels.MainViews
{
    public class OrganicCheckListViewModel : ViewModelBase
    {
        private OrganicScanDataMainView organicScanData = new();
        private readonly VoqooeDataStore voqooeData;

        private bool isLoading = true;
        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }
        public OrganicScanDataMainView OrganicScanData {  get => organicScanData; private set {  organicScanData = value; OnPropertyChanged(nameof(OrganicScanData)); } }

        public OrganicCheckListViewModel(VoqooeDataStore voqooeData)
        {
            this.voqooeData = voqooeData;

            voqooeData.OnOrganicDataChanged += OnOrganicDataChanged;
            voqooeData.OnCurrentCommanderChanged += OnCommanderChanged;
            voqooeData.ReadyStatusChange += OnReadyStateChange;
            var organicData = voqooeData.BioData.Values.ToList();
            OnOrganicDataChanged(null, organicData);
        }

        private void OnReadyStateChange(object? sender, bool e)
        {
            IsLoading = e;
        }

        public override void Dispose()
        {
            voqooeData.OnOrganicDataChanged -= OnOrganicDataChanged;
            voqooeData.OnCurrentCommanderChanged -= OnCommanderChanged;
            voqooeData.ReadyStatusChange -= OnReadyStateChange;
            base.Dispose();
        }

        private void OnCommanderChanged(object? sender, JournalCommander e)
        {
            OrganicScanData = new();
            var organicData = voqooeData.BioData.Values.ToList();
            OnOrganicDataChanged(null, organicData);
        }

        private void OnOrganicDataChanged(object? sender, IEnumerable<OrganicScanDetails> e)
        {
            OrganicScanData.AddScanDetatil(e);
            OnPropertyChanged(nameof(OrganicScanData));
        }
    }
}
