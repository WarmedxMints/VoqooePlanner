using ODUtils.Commands;
using ODUtils.Dialogs;
using VoqooePlanner.Stores;
using VoqooePlanner.ViewModels.MainViews;

namespace VoqooePlanner.Commands
{
    public sealed class LoadVoqooeDataCommand(VoqooeListViewModel model, VoqooeDataStore store, LoggerStore loggerStore) : AsyncCommandBase
    {
        private readonly VoqooeListViewModel model = model;
        private readonly VoqooeDataStore store = store;
        private readonly LoggerStore loggerStore = loggerStore;

        public override async Task ExecuteAsync(object? parameter)
        {
            model.IsLoading = true;

            try
            {
                await store.Load();

                model.UpdateSystemList();
            }
            catch (Exception ex)
            {
                loggerStore.LogError("Load Voqooe Data", ex);
                _ = ODMessageBox.Show(null, "Error Loading Voqooe Data", $"See VPLog.txt in {App.BaseDirectory} for details");
            }

            if(store.Ready)
                model.IsLoading = false;
        }
    }
}
