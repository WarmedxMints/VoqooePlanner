using ODUtils.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;
using VoqooePlanner.Models;
using VoqooePlanner.Stores;
using VoqooePlanner.ViewModels.ModelViews;

namespace VoqooePlanner.ViewModels.MainViews
{
    public class OrganicCheckListViewModel : ViewModelBase
    {
        private OrganicScanDataMainView organicScanData = new();
        private readonly VoqooeDataStore voqooeData;
        private readonly SettingsStore settingsStore;
        private readonly ObservableCollection<OrganicDetailViewModel> organicDetails;
        private readonly ObservableCollection<OrganicDetailsCountViewModel> organicDetailsCount;
        private string organicTosellCount = string.Empty;
        private string totalBiosToSell = string.Empty;
        private string totalBiosToSellValue = string.Empty;
        private bool isLoading = true;

        public string TotalBiosToSell { get => totalBiosToSell; set { totalBiosToSell = value; OnPropertyChanged(nameof(TotalBiosToSell)); } }
        public string TotalBiosToSellValue { get => totalBiosToSellValue; set { totalBiosToSellValue = value; OnPropertyChanged(nameof(TotalBiosToSellValue)); } }

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

        public ExoBiologyViewState CurrentState 
        {
            get
            {
                if (IsLoading == false)
                {
                    return ExoBiologyViewState.Loading;
                }
                return settingsStore.BiologyViewState;
            }
            set
            { 
                settingsStore.BiologyViewState = value; 
                OnPropertyChanged(nameof(CurrentState)); 
            } 
        }

        public ObservableCollection<OrganicDetailViewModel> OrganicDetails => organicDetails;
        public ObservableCollection<OrganicDetailsCountViewModel> OrganicDetailsCounts => organicDetailsCount;
        public string OrganicToSellCount { get => organicTosellCount; private set { organicTosellCount = value; OnPropertyChanged(nameof(OrganicToSellCount)); } }

        public ICommand SwitchToCheckList { get; }
        public ICommand SwitchToUnSoldList { get; }

        public OrganicCheckListViewModel(VoqooeDataStore voqooeData, SettingsStore settingsStore)
        {
            this.voqooeData = voqooeData;
            this.settingsStore = settingsStore;
            organicDetails = [];
            organicDetailsCount = [];

            voqooeData.OnOrganicDataChanged += OnOrganicDataChanged;
            voqooeData.OnCurrentCommanderChanged += OnCommanderChanged;
            voqooeData.ReadyStatusChange += OnReadyStateChange;
            voqooeData.OnOrganicToSellDataChanged += OnOrganicDataToSellChanged;

            SwitchToCheckList = new RelayCommand(OnSwitchToCheckList, (_) => CurrentState != ExoBiologyViewState.CheckList && IsLoading);
            SwitchToUnSoldList = new RelayCommand(OnSwitchToUnSoldList, (_) => CurrentState != ExoBiologyViewState.UnSoldList && IsLoading);

            OnReadyStateChange(null, voqooeData.Ready);
        }

        private void OnSwitchToUnSoldList(object? obj)
        {
            CurrentState = ExoBiologyViewState.UnSoldList;
            OnPropertyChanged(nameof(CurrentState));
        }

        private void OnSwitchToCheckList(object? obj)
        {
            CurrentState = ExoBiologyViewState.CheckList;
            OnPropertyChanged(nameof(CurrentState));
        }

        private void OnOrganicDataToSellChanged(object? sender, System.EventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                organicDetails.Clear();

                if (!voqooeData.OrganicDataToSell.Any())
                {
                    OrganicToSellCount = "None to Sell";
                    organicDetailsCount.Clear();
                    totalBiosToSell = string.Empty;
                    totalBiosToSellValue = string.Empty;
                    OnPropertyChanged(nameof(OrganicDetails));
                    OnPropertyChanged(nameof(OrganicDetailsCounts));
                    OnPropertyChanged(nameof(TotalBiosToSell));
                    OnPropertyChanged(nameof(TotalBiosToSellValue));
                    return;
                }

                var details = voqooeData.OrganicDataToSell.ToList();

                details.Sort((x, y) =>
                {
                    return x.Name.CompareTo(y.Name);
                });

                foreach (var organic in details)
                {
                    organicDetails.Add(new(organic));
                }

                OrganicToSellCount = $"{organicDetails.Count} Samples to Sell";

                var groups = organicDetails.GroupBy(x => x.Species_Local);

                organicDetailsCount.Clear();
                long total = 0;
                long totalcount = 0;
                foreach (var group in groups)
                {
                    var value = group.Sum(x => x.Value);
                    var count = group.Count();
                    total += value;
                    totalcount += count;
                    organicDetailsCount.Add(new(group.Key, count, value));
                }

                totalBiosToSell = totalcount.ToString("N0");
                totalBiosToSellValue = total.ToString("N0");

                OnPropertyChanged(nameof(OrganicDetails));
                OnPropertyChanged(nameof(OrganicDetailsCounts));
                OnPropertyChanged(nameof(TotalBiosToSell));
                OnPropertyChanged(nameof(TotalBiosToSellValue));
            });            
        }      

        private void OnReadyStateChange(object? sender, bool e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                IsLoading = e;
                if (!e)
                {
                    OnPropertyChanged(nameof(CurrentState));
                    return;
                }
                var organicData = voqooeData.BioData.Values.ToList();
                OnOrganicDataChanged(null, organicData);
                OnOrganicDataToSellChanged(null, System.EventArgs.Empty);
                OnPropertyChanged(nameof(CurrentState));
            });
        }

        public override void Dispose()
        {
            voqooeData.OnOrganicDataChanged -= OnOrganicDataChanged;
            voqooeData.OnCurrentCommanderChanged -= OnCommanderChanged;
            voqooeData.ReadyStatusChange -= OnReadyStateChange;
            voqooeData.OnOrganicToSellDataChanged -= OnOrganicDataToSellChanged;
            base.Dispose();
        }

        private void OnCommanderChanged(object? sender, JournalCommander e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                OrganicScanData = new();
                var organicData = voqooeData.BioData.Values.ToList();
                OnOrganicDataChanged(null, organicData);
                OnOrganicDataToSellChanged(null, System.EventArgs.Empty);
            });
        }

        private void OnOrganicDataChanged(object? sender, IEnumerable<OrganicScanDetails> e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                OrganicScanData.AddScanDetatil(e);
                OnPropertyChanged(nameof(OrganicScanData));
            });
        }
    }
}
