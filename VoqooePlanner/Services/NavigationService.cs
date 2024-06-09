using VoqooePlanner.Stores;
using VoqooePlanner.ViewModels;

namespace VoqooePlanner.Services
{
    public sealed class NavigationService<TViewModel>(NavigationStore navigationStore, Func<TViewModel> createViewModel) where TViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore = navigationStore;
        private readonly Func<TViewModel> _createViewModel = createViewModel;

        public ViewModelBase? Current { get { return _navigationStore.CurrentViewModel; } }
        public void Navigate()
        {
            _navigationStore.CurrentViewModel = _createViewModel();
        }
    }
}
