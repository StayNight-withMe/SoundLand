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

      
        public Visibility VisibleTrackListView { get => _visibleTrackListView; set { _visibleTrackListView = value; OnPropertyChanged(); } }
        public ObservableCollection<Track> Tracks { get => _tracks; set { _tracks = value; OnPropertyChanged(); } }
        public PlayList PlayList { get => _playList; set => _playList = value ?? _playList; }
        public Track SelectedTrack { get => _selectedTrack; set { _selectedTrack = value; OnPropertyChanged();  } }

        private PlayState _states;
        public PlayState State { get => _states; set { _states = value; OnPropertyChanged(); OnPropertyChanged(nameof(PlayPauseButtonText)); } }

        public string PlayPauseButtonText => State switch
        {
            PlayState.Play => "Play",
            PlayState.Pause => "Pause",


        };


        public TrackOfPlayListView(TrackCollectionService collectionService, PathService pathService, IAudioFileNameParser audioFileNameParser, IDirectoryService directoryService, IPlayListService playListService)
                    : base(audioFileNameParser,
             playListService, pathService, directoryService)
        {
            //    GetPath getPath = pathService.ParseAll();


            collectionService.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(TrackCollectionService.playList))
                {
                    PlayList = collectionService.playList;
                    Debug.WriteLine($"{collectionService.playList.Directory} переданный плейлист");
             

                    var tracks = collectionService.GetTracks(Path.Combine(PlayList.Directory, @"img"), audioFileNameParser) ?? new List<Track>();
                    Tracks = new ObservableCollection<Track>(tracks);
                    collectionService.Collection = Tracks;
                }
            };






           
        }


        public void SelectedTrackHandler(Track track)
        {
            Debug.WriteLine(track.Name);

            

        }


    }
}
