using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace VoqooePlanner.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        #region Property Changed
        // Declare the event
        public event PropertyChangedEventHandler? PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            if (Application.Current is null)
            {
                return;
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                }
                catch (Exception e) 
                { 
                    Console.WriteLine(e.Message); 
                }
            });
        }
        #endregion

        public virtual void Dispose() { }
    }
}