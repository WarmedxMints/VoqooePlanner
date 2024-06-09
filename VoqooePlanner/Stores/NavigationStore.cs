using VoqooePlanner.ViewModels;

namespace VoqooePlanner.Stores
{
    public sealed class NavigationStore
    {
        private ViewModelBase? _currentViewModel;

        public ViewModelBase? CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel?.Dispose();
                _currentViewModel = value;
                OnViewModelChanged();
            }
        }

        public event Action? CurrentViewModelChanged;
        private void OnViewModelChanged()
        {
            CurrentViewModelChanged?.Invoke();
        }
    }
}
