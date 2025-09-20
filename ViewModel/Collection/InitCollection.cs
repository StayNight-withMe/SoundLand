using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using test.Model; 

namespace test.ViewModel.CollectionClass 
{ 
    public class InitCollection : INotifyPropertyChanged
    {

        private static ObservableCollection<PlayList> _sharedPlayLists = new();

        
        public ObservableCollection<PlayList> PlayLists { get => _sharedPlayLists; set { _sharedPlayLists = value; OnPropertyChanged(); } }


        private ObservableCollection<Track> _allTracks;
        private ObservableCollection<PlayList> _playLists;
        private ObservableCollection<Track> _tracks; 
        public InitCollection()
        {
            _allTracks = new ObservableCollection<Track>();
            _playLists = new ObservableCollection<PlayList>();
            _tracks = new ObservableCollection<Track>(); 
        }

       
        public ObservableCollection<Track> ALLTracks
        {
            get => _allTracks;
            set
            {
                _allTracks = value;
                OnPropertyChanged(); 
            }
        }

        //public ObservableCollection<PlayList> PlayLists
        //{
        //    get => _playLists;
        //    set
        //    {
        //        _playLists = value;
        //        OnPropertyChanged();
        //    }
        //}

        public ObservableCollection<Track> Tracks 
        {
            get => _tracks;
            set
            {
                _tracks = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}