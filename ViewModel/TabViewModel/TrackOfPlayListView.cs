using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using test.Services;
using test.ViewModel.CollectionClass;
using PlayState = test.ViewModel.enamS.PlayPauseButtonStates;

namespace test.ViewModel
{
    public class TrackOfPlayListView : BaseViewModel
    {
        public IMediaService MediaService;

        private PlayList _playList;

        private Visibility _visibleTrackListView;

        private ObservableCollection<Track> _tracks;

        private Track _selectedTrack;

        private string _imgPath;

        public string ImgPath { get => _imgPath; set { _imgPath = value; OnPropertyChanged(); } }
        public Visibility VisibleTrackListView { get => _visibleTrackListView; set { _visibleTrackListView = value; OnPropertyChanged(); } }
        public ObservableCollection<Track> Tracks { get => _tracks; set { _tracks = value; OnPropertyChanged(); } }
        public PlayList PlayList { get => _playList; set { _playList = value ?? _playList; OnPropertyChanged(); } }
        public Track SelectedTrack
        {
            get => _selectedTrack;
            set
            {
                _selectedTrack = value;
                OnPropertyChanged();

                if (value != null)
                {
                    OnTrackSelected(value);
                }
            }
        }
        private PlayState _states;
        public PlayState State { get => _states; set { _states = value; OnPropertyChanged(); OnPropertyChanged(nameof(PlayPauseButtonText)); } }

        public string PlayPauseButtonText  => State switch
        {
            PlayState.Play => "Play",
            PlayState.Pause => "Pause",
        };


        public ICommand PlayPause {  get; set; }
        public TrackOfPlayListView(
       ITrackCollectionService collectionService,  
       IPathService pathService,
       IAudioFileNameParser audioFileNameParser,
       IDirectoryService directoryService,
       IPlayListService playListService)
        {
            MediaService = new MediaService();


            if (collectionService is INotifyPropertyChanged npc)
            {
                Debug.WriteLine("подписка почти");
                npc.PropertyChanged += (sender, e) =>
                {
                    Debug.WriteLine("подписка почти");
                    Debug.WriteLine($"PropertyChanged: {e.PropertyName}");

                    if (e.PropertyName == nameof(ITrackCollectionService.playList))
                    {
                        var service = (ITrackCollectionService)sender;
                        PlayList = service.playList;

                        if (PlayList != null)
                        {
                            Debug.WriteLine($"{PlayList.Directory} переданный плейлист");

                            var tracks = service.GetTracks(
                                Path.Combine(PlayList.Directory, @"img"),
                                audioFileNameParser) ?? Enumerable.Empty<Track>();

                            Tracks = new ObservableCollection<Track>(tracks);
                            VisibleTrackListView = Visibility.Visible;

                            Debug.WriteLine($"Tracks загружены: {Tracks.Count} элементов");
                        }
                    }
                };
                    
            }

            VisibleTrackListView = Visibility.Visible;

            PlayPause = new RelayCommand<object>(_ => PlayPauseHandler());
        }


        private void PlayPauseHandler()
        {
            if (State == PlayState.Pause)
            {
                MediaService.Stop();
                State = PlayState.Play;

            }
            else if (State == PlayState.Play)
            {
                MediaService.Start();
                State = PlayState.Pause;

            }
        }


        public void OnTrackSelected(Track track)
        {
            Debug.WriteLine(track.Name);

            ImgPath = track.ImgFilePath; 

        }


    }
}
