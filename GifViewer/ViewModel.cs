using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GifViewer.Annotations;

namespace GifViewer
{
    public class ViewModel : INotifyPropertyChanged
    {
        private string _search;
        private string[] _urls;
        private readonly DelegateCommand _searchCommand;

        public ViewModel()
        {
            _searchCommand = new DelegateCommand(async () => await LoadGifUrls(), () => !string.IsNullOrEmpty(SearchQuery));
         }

        public string[] Urls
        {
            get => _urls;
            set
            {
                _urls = value;
                OnPropertyChanged();
            }
        }

        public string SearchQuery
        {
            get => _search;
            set
            {
                _search = value;
                OnPropertyChanged();
                _searchCommand.RaiseCanExecuteChanged();
            }
        }

        public ICommand Search => _searchCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task LoadGifUrls()
        {
            try
            {
                IGifsClient client = new GifsClient();
                Urls = (await client.SearchAsync(SearchQuery)).Take(3).ToArray(); //Limit by 3 for not freezing UI
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "ERROR");
            }
        }
    }
}
