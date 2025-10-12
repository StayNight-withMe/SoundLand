using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Security.Policy;
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

        private Track _tempchoice;

        public IMediaService MediaService { get { return _mediaService; Debug.WriteLine("МедиаСервисберем"); } set { _mediaService = value; } }
        public string SourceForMediaElement { get => _sourceForMediaElement; set { _sourceForMediaElement = value; OnPropertyChanged(); } }
        public string ImgPath { get => _imgPath; set { _imgPath = value; OnPropertyChanged(); } }
        public Visibility VisibleTrackListView { get => _visibleTrackListView; set { _visibleTrackListView = value; OnPropertyChanged(); } }
        public ObservableCollection<Track> Tracks { get => _tracks; set { _tracks = value; OnPropertyChanged(); } }
        public PlayList PlayList { get => _playList; set { _playList = value ?? _playList; OnPropertyChanged(); } }
        public Track SelectedTrack { get => _selectedTrack; set { _selectedTrack = value; OnPropertyChanged(); if (value != null) { OnTrackSelected(value); } } }
        public PlayState State { get => _states; set { _states = value; OnPropertyChanged(); OnPropertyChanged(nameof(PlayPauseButtonText)); } }

        private string _songName;
        public string SongName { get => _songName; set { _songName = value; OnPropertyChanged(); } }

        private double _songSliderValue;
        public double SongSliderValue
        {
            get => _songSliderValue; set
            {
                _songSliderValue = value; OnPropertyChanged(); if (TotalSeconds > 0)
                {
                    double seconds = (value / 100) * TotalSeconds;
                    _mediaService.Seek(seconds);
                }
            }
        }

        private double _totalSeconds;
        public double TotalSeconds
        {
            get => _totalSeconds;
            set
            {
                _totalSeconds = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SecondForView));
            }
        }

        public string SecondForView { get { TimeSpan time = TimeSpan.FromSeconds(_totalSeconds); ; return time.ToString(@"mm\:ss"); } set { } }



        public string PlayPauseButtonText  => State switch
        {
            PlayState.Play => "Play",
            PlayState.Pause => "Pause",
        };


        public ICommand PlayPause {  get; set; }
        public ICommand NextTrack { get; set; }
        public ICommand PreviousTrack { get; set; }
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

            _mediaService.PositionChanged += OnPositionChanged;

            _mediaService.DurationChanged += OnDurationChanged;

            SubOnCollecion();

            VisibleTrackListView = Visibility.Visible;

            PlayPause = new RelayCommand<object>(_ => PlayPauseHandler());
            NextTrack = new RelayCommand<object>(_ => NextorPreviousTrackHandler(true));
            PreviousTrack = new RelayCommand<object>(_ => NextorPreviousTrackHandler(false));
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



        private void OnDurationChanged(double duration)
        {
            TotalSeconds = duration;
        }

        private void OnPositionChanged(double position)
        {
            if (_mediaService.TotalSeconds > 0)
            {

                SongSliderValue = (position / _mediaService.TotalSeconds) * 100;
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


        public void NextorPreviousTrackHandler(bool skip)
        {
            if (Tracks == null || Tracks.Count == 0 || _tempchoice == null)
                return;

            int currentIndex = Tracks.IndexOf(_tempchoice);

            if (currentIndex == -1)
            {
                // ✅ Текущий трек не найден - начинаем с первого:
                StartSong(Tracks[0]);
                return;
            }

            int newIndex;

            if (skip)
            {
                // ✅ Следующий трек (циклически):
                newIndex = (currentIndex + 1) % Tracks.Count;
            }
            else
            {
                // ✅ Предыдущий трек (циклически):
                newIndex = (currentIndex - 1 + Tracks.Count) % Tracks.Count;
            }

            StartSong(Tracks[newIndex]);
        }



        public void StartSong(Track track)
        {
            _tempchoice = track;

            SongName = track.Name;
            ImgPath = track.ImgFilePath;

            var i = Path.Combine(_getPath.PlayListPath, _trackCollectionService.playList.Name);

            var ii = Path.Combine(i, "song", track.FileName + ".mp3");

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

            State = PlayState.Pause;
        }

        public async void  OnTrackSelected(Track track)
        {
         
            Debug.WriteLine($"{track.Name} трек");

            StartSong(track);



        }


    }
}
