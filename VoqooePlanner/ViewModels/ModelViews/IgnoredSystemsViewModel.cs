using System.Security.Policy;
using VoqooePlanner.Models;

namespace VoqooePlanner.ViewModels.ModelViews
{
    public sealed class IgnoredSystemsViewModel(IgnoredSystem ignoredSystem) : ViewModelBase
    {
        private bool restore;

        public long Address => ignoredSystem.Address;
        public string Name => ignoredSystem.Name;
        public int CmdrId => ignoredSystem.CmdrId;
        public bool Restore
        {
            get => restore;
            set
            {
                restore = value;
                OnPropertyChanged(nameof(Restore));
            }
        }
    }
}
