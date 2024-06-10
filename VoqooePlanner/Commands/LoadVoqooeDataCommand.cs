using ODUtils.Commands;
using VoqooePlanner.Stores;
using VoqooePlanner.ViewModels.MainViews;

namespace VoqooePlanner.Commands
{
    public sealed class LoadVoqooeDataCommand(VoqooeListViewModel model, VoqooeDataStore store) : AsyncCommandBase
    {
        private readonly VoqooeListViewModel model = model;
        private readonly VoqooeDataStore store = store;

        public override async Task ExecuteAsync(object? parameter)
        {
            model.IsLoading = true;

            try
            {
                await store.Load();

                model.UpdateSystemList();
            }
            catch (Exception)
            {

            }

            if(store.Ready)
                model.IsLoading = false;
        }
    }
}
