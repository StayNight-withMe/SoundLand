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


namespace test.ViewModel
{
    public class TrackOfPlayListView : BaseViewModel
    {
        private readonly IPathService _pathService;

        private readonly IAudioFileNameParser _audioFileNameParser;

        private PlayList _playList;

        private ObservableCollection<Track> _tracks;

        private Visibility _visibleTrackListView;
        public Visibility VisibleTrackListView { get => _visibleTrackListView; set { _visibleTrackListView = value; OnPropertyChanged(); } }


      
        public ObservableCollection<Track> Tracks { get => _tracks; set { _tracks = value; OnPropertyChanged(); } }

        

        public PlayList PlayList { get => _playList; set => _playList = value ?? _playList; }


        public TrackOfPlayListView(TrackCollectionService collectionService, PathService pathService, IAudioFileNameParser audioFileNameParser)
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


    }
}
