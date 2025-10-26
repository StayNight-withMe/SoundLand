using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using test.Services;
using test.Services.PlayListService.Interfaces;
using test.ViewModel.CollectionClass;
using static test.ViewModel.enamS;
using NextTrackState = test.ViewModel.enamS.NextMediaStates;
using PlayState = test.ViewModel.enamS.PlayPauseButtonStates;

namespace test.ViewModel
{
    public sealed record DataUpdateMessage(string imageSource,  string TrackName, string Artist);
    public class TrackOfPlayListView : BaseViewModel, IRecipient<DataUpdateMessage>
    {

        private readonly IPlayListServiceInside _playListServicel;
        
        private IMediaService _mediaService;

        private PlayList _playList;

        private Visibility _visibleTrackListView;

        private PlayState _states;

        private ObservableCollection<Track> _tracks;

        private Track _selectedTrack;

        private string _imgPath;

        private string _sourceForMediaElement;

        private Track _tempchoice;
        public readonly IMessenger _messenger;
        public IMediaService MediaService { get { return _mediaService; } set { _mediaService = value; OnPropertyChanged(); } }
        public string SourceForMediaElement { get => _sourceForMediaElement; set { _sourceForMediaElement = value; OnPropertyChanged(); } }
        public  string ImgPath { get => _imgPath; set { 
                _imgPath = value; OnPropertyChanged(); 
            } }
        public Visibility VisibleTrackListView { get => _visibleTrackListView; set { _visibleTrackListView = value; OnPropertyChanged(); } }
        public ObservableCollection<Track> Tracks { get => _tracks; set { _tracks = value; OnPropertyChanged(); } }
        public PlayList PlayList { get => _playList; set { _playList = value ?? _playList; OnPropertyChanged(); Debug.WriteLine($"Выбранный плейлист : {_playList}"); } }
        public Track SelectedTrack { get => _selectedTrack; set { _selectedTrack = value; OnPropertyChanged(); if (value != null) { OnTrackSelected(value); } } }
        public PlayState State { get => _states; set { _states = value; OnPropertyChanged(); OnPropertyChanged(nameof(PlayPauseButtonText)); } }

        private Dispatcher _uiDispatcher;

        private string _songName;
        public string SongName { get => _songName; set { _songName = value; OnPropertyChanged(); } }

        private double _songSliderValue;
        public double SongSliderValue
        {
            get => _songSliderValue; 
            set {
                _songSliderValue = value; OnPropertyChanged(); if (TotalSeconds > 0)
                {
                    double seconds = (value / 100) * TotalSeconds;
                    _mediaService.Seek(seconds);
                    OnPropertyChanged(nameof(SecondProcess));
                }
            }
        }

        private string _songArtist;
        public string SongArtist { get => _songArtist; set { _songArtist = value; OnPropertyChanged(); } }

        private double _totalSeconds;
        public double TotalSeconds
        {
            get => _totalSeconds;
            set
            {
                _totalSeconds = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SecondForView));
                OnPropertyChanged(nameof(SecondProcess));
            }
        }

        public string SecondForView { get { TimeSpan time = TimeSpan.FromSeconds(_totalSeconds); return time.ToString(@"mm\:ss"); } }

        public string SecondProcess { get { double currentSeconds = (_songSliderValue / 100) * _totalSeconds; TimeSpan time = TimeSpan.FromSeconds(currentSeconds); return time.ToString(@"mm\:ss"); } }

        private NextTrackState _playModeContent = NextTrackState.Next;

        public string PlayModeSymbol => _playModeContent switch
        {
            NextTrackState.Next => "🔁",
            NextTrackState.Random => "🔀",
            NextTrackState.Replay => "🔂",
            _ => "🔁"
        };

        public NextTrackState PlayModeState
        {
            get => _playModeContent;
            set
            {
                _playModeContent = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PlayModeSymbol));  
            }
        }


        public string PlayPauseButtonText  => State switch
        {
            PlayState.Play => "Play",
            PlayState.Pause => "Pause",
        };


        public ICommand PlayPause {  get; set; }
        public ICommand NextTrack { get; set; }
        public ICommand PreviousTrack { get; set; }
        public ICommand PlayModeHander { get; set; }
        public ICommand DeleteTrack { get; set; }

        public TrackOfPlayListView(
            Dispatcher uiDispatcher,
       ITrackCollectionService collectionService,  
       IPathService pathService,
       IAudioFileNameParser audioFileNameParser,
       IDirectoryService directoryService,
       IPlayListServiceInside playListService,
       IMessenger messenger  )
        {
            _mediaService = new MediaService();

            _messenger = messenger;

            _uiDispatcher = uiDispatcher;

            _playListServicel = playListService;

            _audioFileNameParser = audioFileNameParser;

            _directoryService = directoryService;

            _getPath = pathService.ParseAll();

            _trackCollectionService = collectionService;

            _mediaService.PositionChanged += OnPositionChanged;

            _mediaService.DurationChanged += OnDurationChanged;

            _mediaService.MediaEndedChanged += OnMediaEndedChanged;

            SubOnCollecion();

            VisibleTrackListView = Visibility.Visible;

            PlayPause = new RelayCommand<object>(_ => PlayPauseHandler());
            NextTrack = new RelayCommand<object>(_ => NextorPreviousTrackHandler(true));
            PreviousTrack = new RelayCommand<object>(_ => NextorPreviousTrackHandler(false));
            PlayModeHander = new RelayCommand<object>(_ => ChangeStatePlayMode());
            DeleteTrack = new RelayCommand<Track>(DeleteTrackHandler);
            ImgPath = Constants.defaultImagePath;

            _messenger.Register<DataUpdateMessage>(this);

        }



        public void Receive(DataUpdateMessage message)
        {
            ImgPath = message.imageSource;
            SongName = message.TrackName;
            SongArtist = message.Artist;
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


        private void ChangeStatePlayMode()
        {
            PlayModeState = PlayModeState switch
            {
                NextTrackState.Next => NextTrackState.Random,
                NextTrackState.Random => NextTrackState.Replay,
                NextTrackState.Replay => NextTrackState.Next,
                _ => NextTrackState.Next
            };


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

        private void OnMediaEndedChanged()
        {

            // NextorPreviousTrackHandler(true);
            switch (PlayModeState)
            {
                case NextTrackState.Next:
                    NextorPreviousTrackHandler(true);
                    break;

                case NextTrackState.Random:
                    Random random = new Random();
                    var i = random.Next(0, Tracks.Count);
                    StartSong(Tracks[i]);

                    break;

                case NextTrackState.Replay:
                    StartSong(_tempchoice);

                    break;
            }
        }





        public async void DeleteTrackHandler(Track track)
        {
            if(track.Equals(SelectedTrack))
            {
                MessageBox.Show("Нельзя удалить трек, пока он выбран, \n пожалуйста, выберите другой трек");
                return;
            }
            if (track == null)
            {
                Debug.WriteLine("Все хуево");
                return;
            }
            Debug.WriteLine("запуск удаления");
            await _playListServicel.DeleteTrackFromPlayList(track, PlayList);
            Debug.WriteLine($"попытка удалить : {track.Name}");
            Tracks.Clear();
            Debug.WriteLine(_playList.Name);
            if(_uiDispatcher == null)
            {
                Debug.WriteLine("нет диспетчера епта");
                return;
            }
            await _uiDispatcher.InvokeAsync(() =>
            {
                Tracks = _trackCollectionService.GetTracks(
                                Path.Combine(PlayList.Directory, @"img"), _audioFileNameParser);
            });
        

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

            if(PlayModeState == NextTrackState.Random)
            {
                Random random = new Random();

                MediaService.Seek(0);
                var i = random.Next(0, Tracks.Count);
                Debug.WriteLine(i);
                StartSong(Tracks[i]);
                Debug.WriteLine("включился рандомный трек вроде");
                return;
                


            }

            int currentIndex = Tracks.IndexOf(_tempchoice);

            if (currentIndex == -1)
            { 
                StartSong(Tracks[0]);
                return;
            }

            int newIndex;

            if (skip)
            {

                newIndex = (currentIndex + 1) % Tracks.Count;
            }
            else
            {
                
                newIndex = (currentIndex - 1 + Tracks.Count) % Tracks.Count;
            }

            StartSong(Tracks[newIndex]);
        }



        public void StartSong(Track track)
        {
            _tempchoice = track;

            SongName = track.Name;
            SongArtist = track.Artist;
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

        public void OnTrackSelected(Track track)
        {
            MediaService.Seek(0);
            Debug.WriteLine($"{track.Name} трек");

            StartSong(track);



        }


    }
}
