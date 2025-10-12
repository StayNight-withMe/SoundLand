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
using System.Windows.Media;
using test.Services;
using test.ViewModel.CollectionClass;
using static test.ViewModel.enamS;
using PlayState = test.ViewModel.enamS.PlayPauseButtonStates;

namespace test.ViewModel
{
    public class TrackOfPlayListView : BaseViewModel
    {
        private IMediaService _mediaService;

        private PlayList _playList;

        private Visibility _visibleTrackListView;

        private PlayState _states;

        private ObservableCollection<Track> _tracks;

        private Track _selectedTrack;

        private string _imgPath;

        private string _sourceForMediaElement;


        public IMediaService MediaService { get { return _mediaService; Debug.WriteLine("МедиаСервисберем"); } set { _mediaService = value; } }
        public string SourceForMediaElement { get => _sourceForMediaElement; set { _sourceForMediaElement = value; OnPropertyChanged(); } }
        public string ImgPath { get => _imgPath; set { _imgPath = value; OnPropertyChanged(); } }
        public Visibility VisibleTrackListView { get => _visibleTrackListView; set { _visibleTrackListView = value; OnPropertyChanged(); } }
        public ObservableCollection<Track> Tracks { get => _tracks; set { _tracks = value; OnPropertyChanged(); } }
        public PlayList PlayList { get => _playList; set { _playList = value ?? _playList; OnPropertyChanged(); } }
        public Track SelectedTrack { get => _selectedTrack; set { _selectedTrack = value; OnPropertyChanged(); if (value != null) { OnTrackSelected(value); } } }
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
            _audioFileNameParser = audioFileNameParser;

            _directoryService = directoryService;

            _getPath = pathService.ParseAll();

            _trackCollectionService = collectionService;

            _mediaService = new MediaService();

            SubOnCollecion();

            VisibleTrackListView = Visibility.Visible;

            PlayPause = new RelayCommand<object>(_ => PlayPauseHandler());
        }


        private void SubOnCollecion()
        {
            if (_trackCollectionService is INotifyPropertyChanged npc)
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
                                _audioFileNameParser) ?? Enumerable.Empty<Track>();

                            Tracks = new ObservableCollection<Track>(tracks);
                            VisibleTrackListView = Visibility.Visible;

                            Debug.WriteLine($"Tracks загружены: {Tracks.Count} элементов");
                        }
                    }
                };

            }

        }


        private void PlayPauseHandler()
        {
            if (State == PlayState.Pause)
            {
                _mediaService.Stop();
                State = PlayState.Play;

            }
            else if (State == PlayState.Play)
            {
                _mediaService.Start();
                State = PlayState.Pause;

            }
        }


        public async Task OnTrackSelected(Track track)
        {
            Debug.WriteLine($"{track.Name} трек");

            ImgPath = track.ImgFilePath;

            var i = Path.Combine(_getPath.PlayListPath, _trackCollectionService.playList.Name);

            var ii = Path.Combine(i, "song" ,track.FileName+".mp3");

            Debug.WriteLine(Path.Combine(i, $"{track.FileName}.mp3"));

            SourceForMediaElement = ii;

            if (MediaService?.MediaElement == null)
            {
                MessageBox.Show("медиав-серисс = 0");
            }
            else
            {
                Console.WriteLine("МедиаСервисНорм");
            }

            State = PlayState.Pause;
            if (!File.Exists(SourceForMediaElement))
            {
                Debug.WriteLine($"❌ Файл не найден: {SourceForMediaElement}");
                MessageBox.Show($"Файл не найден: {SourceForMediaElement}");
                return;
            }

            Debug.WriteLine($"Файл найден: {SourceForMediaElement}");

      
            Debug.WriteLine($"URI: {MediaService.MediaElement.Source}");

     
            MediaService.Seek(0);
            MediaService.Start(); 

            Debug.WriteLine($"MediaElement.State: {MediaService.MediaElement.LoadedBehavior}");
            Debug.WriteLine($"MediaElement.IsPlaying: {MediaService.MediaElement.Clock?.CurrentState}");

            State = PlayState.Pause;  // ✅ Изменение состояния


        }


    }
}
