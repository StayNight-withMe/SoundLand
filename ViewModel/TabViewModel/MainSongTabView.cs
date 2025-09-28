using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using test.Services;

using test.ViewModel.CollectionClass;


namespace test.ViewModel.TabViewModel
{
    public class MainSongTabView : BaseViewModel
    {
        
        private readonly IPythonScriptService _pythonScriptService;

        private readonly IAudioFileNameParser _audioFileNameParser;

        private readonly IDirectoryService _directoryService;

        private readonly IPlayListService _playListService;

        private readonly IPathService _pathService;

        private readonly GetPath _getPath;

        private Track _tempChoiceTrack;

        private IMediaService _mediaService;

        private TrackCollectionService _trackCollectionService;

        private bool _popupIsOpen;
        public bool PopupIsOpen { get => _popupIsOpen; set { _popupIsOpen = value; OnPropertyChanged(); } }

        private string? _inputText;
        public string InputText {  get => _inputText; set { _inputText = value; OnPropertyChanged(); }  }

        private Track? _selectedTrack;
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

        private PlayList _selectedPlayList;
        public PlayList SelectedPlayList
        {
            get => _selectedPlayList;
            set { _selectedPlayList = value; OnPropertyChanged(); }
        }

        private string _sourceForMediaElement;
        public string SourceForMediaElement { get => _sourceForMediaElement; set { _sourceForMediaElement = value; OnPropertyChanged(); } }
        public IMediaService MediaService { get => _mediaService; set { _mediaService = value; } }
        
        private string _image;    
        public string Image { get => _image; set { _image = value; OnPropertyChanged(); } }

        private double _songSliderValue;
        public double SongSliderValue { get => _songSliderValue; set { _songSliderValue = value; OnPropertyChanged(); if (TotalSeconds > 0)
                {
                    double seconds = (value / 100) * TotalSeconds;
                    _mediaService.Seek(seconds);
                } /*Debug.WriteLine(_songSliderValue);*/
            } }


        private PlayPauseButtonStates _states;
        public enum PlayPauseButtonStates
        {
            Play,
            Pause,
        }

        public PlayPauseButtonStates State { get => _states; set { _states = value; OnPropertyChanged(); OnPropertyChanged(nameof(PlayPauseButtonText)); } }
        public string PlayPauseButtonText => State switch
        {
            PlayPauseButtonStates.Play => "Play",
            PlayPauseButtonStates.Pause => "Pause"

        };

        private double _totalSeconds;
        public double TotalSeconds
        {
            get => _totalSeconds;
            set
            {
                _totalSeconds = value;
                OnPropertyChanged();
            }
        }

        public ICommand SearchSong { get; set; }
        public ICommand SelectionChanged { get; set; }
        public ICommand ToALLTrack { get; set; }
        public ICommand ToPlayList { get; set; }
        public ICommand AddToPlayList { get; set; }
        public ICommand СancelPopup { get; set; }
        public ICommand PlayPause { get; set; }

        public InitCollection Collections { get; set; }
        public MainSongTabView(IPythonScriptService pythonScriptService, IAudioFileNameParser audioFileNameParser,
            IPlayListService playListService, IPathService pathService, IDirectoryService directoryService, TrackCollectionService trackCollectionService)
        {
            _pathService = pathService;

            _playListService = playListService;

            _audioFileNameParser = audioFileNameParser;

            _pythonScriptService = pythonScriptService;

            _directoryService = directoryService;

            _mediaService = new MediaService();

            _trackCollectionService = trackCollectionService;

            _mediaService.PositionChanged += OnPositionChanged;
            _mediaService.DurationChanged += OnDurationChanged;
      


            ToALLTrack = new RelayCommand<Track>(ToALLTrackHandler);
            SearchSong = new RelayCommand<object>(_ => SearchSongHandler());
            ToPlayList = new RelayCommand<Track>(ToPlayListHandler);
            СancelPopup = new RelayCommand<Object>(_ => PopupIsOpen = false);
            AddToPlayList = new RelayCommand<Object>(_ => AddToPlayListHandler());
            Collections = new InitCollection();
            PlayPause = new RelayCommand<object>(_ => PlayPauseHandler());

            _getPath = _pathService.ParseAll();
            State = PlayPauseButtonStates.Pause;
        }

        private void OnDurationChanged(double duration)
        {
            TotalSeconds = duration;
        }


        private void PlayPauseHandler()
        {
            if (State == PlayPauseButtonStates.Pause)
            {
                State = PlayPauseButtonStates.Play;
                MediaService.Start();
            }
            else
            {
                State = PlayPauseButtonStates.Pause;
                MediaService.Stop();
            }
        }


        private void OnPositionChanged(double position)
        {
            if (_mediaService.TotalSeconds > 0)
            {
                
                SongSliderValue = (position / _mediaService.TotalSeconds) * 100;
            }
        }

        private void ToPlayListHandler(Track track)
        {
            _tempChoiceTrack = track;
            PopupIsOpen = true;
        }

        private async void AddToPlayListHandler()
        {
            
            if (SelectedPlayList != null)
            {
                Debug.WriteLine(_tempChoiceTrack.Name);

                await _pythonScriptService.PythonScript("Untitled-3.py", 1, _inputText, _tempChoiceTrack.FileName, _tempChoiceTrack.Name);

                _playListService.AddTrackToPlayList(SelectedPlayList, _tempChoiceTrack);

            }
            
            PopupIsOpen = false;

        }

      
        private async Task OnTrackSelected(Track selectedItem)
        {
            Debug.WriteLine("Получение элемента");
            if (selectedItem != null)
            {

                var index = Collections.Tracks.IndexOf(selectedItem);
                var allItems = Collections.Tracks.ToList();
                Debug.WriteLine(selectedItem.Name);

                await _pythonScriptService.PythonScript("Untitled-3.py", 1, _inputText, selectedItem.FileName, selectedItem.Name);

                SourceForMediaElement = Path.GetFullPath(selectedItem.SongFilePath);

                Debug.WriteLine(selectedItem.SongFilePath);

                if (MediaService?.MediaElement == null)
                {
                    MessageBox.Show("медиав-серисс = 0");
                    return;
                }

                Image = Path.GetFullPath(selectedItem.ImgFilePath);
                State = PlayPauseButtonStates.Pause;
                MediaService.Start();    
                
            }
        }

        public async Task SearchSongHandler()
        {

            await _directoryService.ClearDirectory(_getPath.TempImgPath);
            await _directoryService.ClearDirectory(_getPath.TempSongPath);
     
            await _pythonScriptService.PythonScript("Untitled-3.py", 2, _inputText, "emp", "emp");

            string[] imgFiles = Directory.GetFiles(_getPath.TempImgPath, "*.jpg");
            Collections.Tracks.Clear();

      
            foreach (var imgFile in imgFiles)
            {
                string fullImgPath = Path.GetFullPath(imgFile);

                FileNameInfo fileInfo = _audioFileNameParser.ParseAll(fullImgPath);

                Debug.WriteLine($"Song: {fileInfo.SongName}, Artist: {fileInfo.SongArtist}, File: {fileInfo.FileName}, Duration: {fileInfo.SongDuration}");

                string imgPath = Path.GetFullPath(imgFile);
                byte[] imageData = File.ReadAllBytes(imgPath);



                Collections.Tracks.Add(new Track
                {
                    Name = fileInfo.SongName,
                    Artist = fileInfo.SongArtist,
                    FileName = Path.GetFileNameWithoutExtension(fileInfo.FileName),
                    Duration = fileInfo.SongDuration,
                    ImageData = imageData,
                    ImgFilePath = fileInfo.ImgFilePath,
                    SongFilePath = fileInfo.SongFilePath,
                });


            }

        }



        public async void ToALLTrackHandler(Track track)
        {

            await _pythonScriptService.PythonScript("Untitled-3.py", 1, _inputText, track.FileName, track.Name);

            string sourceImagePath = Path.GetFullPath(Path.Combine("temp_img", $"{track.FileName}.jpg"));
            string sourceAudioPath = Path.GetFullPath(Path.Combine("temp_song", $"{track.FileName}.mp3"));

            string targetImgDir = Path.Combine(@"ALL\img", $"{track.FileName}.jpg");
            string targetSongDir = Path.Combine(@"ALL\song", $"{track.FileName}.mp3");

            Debug.WriteLine("SOURCEDIR", sourceImagePath, sourceAudioPath);
            Debug.WriteLine("TARGET DIR", targetSongDir, targetImgDir);

            await _directoryService.CopyFileToDerictory(sourceImagePath, targetImgDir);
            await _directoryService.CopyFileToDerictory(sourceAudioPath, targetSongDir);


        }





    }
}


