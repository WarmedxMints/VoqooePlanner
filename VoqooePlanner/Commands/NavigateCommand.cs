using ODUtils.Commands;
using VoqooePlanner.Services;
using VoqooePlanner.ViewModels;

namespace VoqooePlanner.Commands
{
    public sealed class NavigateCommand<TViewModel>(NavigationService<TViewModel> navigationService) : CommandBase where TViewModel : ViewModelBase
    {
        private readonly NavigationService<TViewModel> navigationService = navigationService;

        public override bool CanExecute(object? parameter)
        {
            return navigationService.Current is not TViewModel;
        }
        public override void Execute(object? parameter)
        {
            navigationService.Navigate();
        }
    }
}
